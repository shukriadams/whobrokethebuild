const BaseDaemon = require(_$+'daemon/base')
    
/**
 * @extends {BaseDaemon}
 */
module.exports = class MapRevisions extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){
        // try to map map local users to users in vcs for a given build
        const pluginsManager = require(_$+'helpers/pluginsManager'),
            faultHelper = require(_$+ 'helpers/fault'),
            data = await pluginsManager.getExclusive('dataProvider'),
            logHelper = require(_$+'helpers/log'),
            buildsWithUnprocessedRevisionObjects = await data.getBuildsWithoutRevisionObjects()

        __log.debug(`found ${buildsWithUnprocessedRevisionObjects.length} builds with unmapped revisions`)

        for (const build of buildsWithUnprocessedRevisionObjects){
            try {
                const job = await data.getJob(build.jobId, { expected : true }),
                    vcServer = await data.getVCServer(job.VCServerId, { expected : true }),
                    vcPlugin = await pluginsManager.get(vcServer.vcs, { expected : true })

                // ignore builds that have not yet had their logs fetched
                // ignore jobs that don't have log parsers defined
                if (!build.logPath || !job.logParser)
                    continue

                for (const buildInvolvement of build.involvements){
                    buildInvolvement.revisionObject = await vcPlugin.getRevision(buildInvolvement.revision, vcServer)  
                    
                    // force placeholder revision object if lookup to vc fails to retrieve it
                    if (!buildInvolvement.revisionObject)
                        buildInvolvement.revisionObject = {
                            revision : buildInvolvement.revision,
                            user : buildInvolvement.externalUsername,
                            description : `${buildInvolvement.revision} lookup failed!`,
                            files : [] 
                        }
                    
                    const errorLines = await logHelper.parseErrorsFromBuildLog(build, job.logParser)
                    faultHelper.processRevision(buildInvolvement.revisionObject, errorLines)
                    __log.debug(`Mapped revision ${buildInvolvement.revision} in buildInvolvement ${buildInvolvement.id}`)

                }

                await data.updateBuild(build)

            } catch (ex){
                __log.error(`Unexpected error in ${this.constructor.name} : build "${build.id}"`, ex)
            }
        }


    }
}