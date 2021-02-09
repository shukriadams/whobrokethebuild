const BaseDaemon = require(_$+'daemon/base')
    
/**
 * @extends {BaseDaemon}
 */
module.exports = class MapRevisions extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){

        __log.debug(`mapRevisions daemon doing work ....`)

        // try to map map local users to users in vcs for a given build
        const pluginsManager = require(_$+'helpers/pluginsManager'),
            faultHelper = require(_$+ 'helpers/fault'),
            data = await pluginsManager.getExclusive('dataProvider'),
            unprocessedBuildInvolvements = await data.getBuildInvolvementsWithoutRevisionObjects()

        __log.debug(`found ${unprocessedBuildInvolvements.length} buildInvolvements with unmapped revisions`)

        for (const buildInvolvement of unprocessedBuildInvolvements){
            try {
                const build = await data.getBuild(buildInvolvement.buildId, { expected : true }),
                    job = await data.getJob(build.jobId, { expected : true }),
                    vcServer = await data.getVCServer(job.VCServerId, { expected : true }),
                    vcPlugin = await pluginsManager.get(vcServer.vcs, { expected : true }),
                    logParser = job.logParser ? await pluginsManager.get(job.logParser) : null

                // ignore builds that have not yet had their logs fetched
                // ignore jobs that don't have log parsers defined
                if (!build.log || !logParser)
                    continue
                    
                buildInvolvement.revisionObject = await vcPlugin.getRevision(buildInvolvement.revision, vcServer)  
                
                // force placeholder revision object if lookup to vc fails to retrieve it
                if (!buildInvolvement.revisionObject)
                    buildInvolvement.revisionObject = {
                        revision : `${buildInvolvement.revision} lookup failed`,
                        user : buildInvolvement.externalUsername,
                        description : '', 
                        files : [] 
                    }
                
                const errorLines = logParser.parseErrors(build.log)
                faultHelper.processRevision(buildInvolvement.revisionObject, errorLines)
                await data.updateBuildInvolvement(buildInvolvement)
                
                __log.debug(`Mapped revision ${buildInvolvement.revision} in buildInvolvement ${buildInvolvement.id}`)

            } catch (ex){
                __log.error(`Unexpected error in mapRevisions : buildInvolvement "${buildInvolvement.id}"`, ex)
            }
        }


    }
}