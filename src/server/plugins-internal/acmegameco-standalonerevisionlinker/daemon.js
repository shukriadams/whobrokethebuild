const BaseDaemon = require(_$+'daemon/base')

/**
 * @extends {BaseDaemon}
 */
module.exports = class StandaloneRevisionLinker extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){
        // try to map map local users to users in vcs for a given build
        const fs = require('fs-extra'),
            constants = require(_$+'types/constants'),
            BuildInvolvment = require(_$+'types/buildInvolvement'),
            pluginsManager = require(_$+'helpers/pluginsManager'),
            settings = require(_$+'helpers/settings'),
            path = require('path'),
            data = await pluginsManager.getExclusive('dataProvider'),
            builds = await data.getResolvedBuildsWithNoInvolvements()

        for (const build of builds){
            try {
                const job = await data.getJob(build.jobId, { expected : true }),
                    perforcePLugin = await pluginsManager.get('wbtb-perforce'),
                    previousBuild = await data.getPreviousBuild(build),
                    vcServer = await data.getVCServer(job.VCServerId, { expected : true })

                // ignore builds that have not yet had their logs fetched
                // ignore jobs that don't have log parsers defined
                if (!job.logParser){
                    __log.debug(`No parser defined for job ${job.name}, skipping`)
                    continue
                }

                // NOTE! query ensures that logPath is already defined, but we don't know if the file exists
                const logPath = path.join(settings.buildLogsDump, build.jobId, build.build.toString())
                if (!await fs.pathExists(logPath)){
                    __log.error(`LOG MISSING - Job ${job.name}, build ${build.build} defines log ${build.id}, but the file does not exist`)
                    build.processStatus = 'CANNOT_RESOLVE'
                    await data.updateBuild(build)
                    continue
                }

                const log = await fs.readFile(logPath, 'utf8'),
                    regex = new RegExp('\#p4-changes........\#\n(.*)\n\#/p4-changes........\#'),
                    lookup = log.match(regex)

                if (!lookup){
                    __log.info(`Job ${job.name}, build ${build.build} log is missing "p4-changes" block, add this to build logs to enable revision range matching`)
                    build.processStatus = 'CANNOT_RESOLVE'
                    await data.updateBuild(build)
                    continue
                }

                // lookup will look like "Change REVNR on ...."
                const revnrLookup = lookup.pop().match(/^Change (.*) on /)
                if (!revnrLookup)
                    continue

                let knownRevisionInThisBuild = revnrLookup.pop(),
                    revisionsBefore = await perforcePLugin.getRevisionsBefore(knownRevisionInThisBuild),
                    revisionsInThisBuild = []

                revisionsInThisBuild.push(knownRevisionInThisBuild)

                if (revisionsBefore.length && previousBuild) {
                    let lastRevision = 0

                    // find latest revision in previous build
                    for (const b of previousBuild.involvements)
                        if (parseInt(b.revision) > lastRevision)
                            lastRevision = parseInt(b.revision)
                    
                    // find all revisions from range lookup that were created AFTER the previous build's last revision and 
                    // BEFORE this build's known revision. This should give us all revisions that are in this one.
                    if (lastRevision)
                        for (const revisionData of revisionsBefore)
                            if (revisionData.revision > lastRevision && revisionData.revision < knownRevisionInThisBuild)
                                revisionsInThisBuild.push(revisionData.revision)
                }

                if (revisionsInThisBuild.length){
                    build.revisions = revisionsInThisBuild
                    build.comment = build.comment || ''
                    build.comment += `\n ${revisionsInThisBuild.length} revisions were soft-matched in based on #${knownRevisionInThisBuild} appearing in build log`
                }
    
                for (const revisionNr of revisionsInThisBuild){
                    const revisionData = await perforcePLugin.getRevision(revisionNr, vcServer)
                    if (revisionData){
                        let revisionId = revisionNr.toString()
                        if (build.involvements.find(r => r.revisionId === revisionId))
                            continue 
    
                        const buildInvolvment = new BuildInvolvment()
                        buildInvolvment.externalUsername = revisionData.user
                        buildInvolvment.buildId = build.id
                        buildInvolvment.revision = revisionId
                        buildInvolvment.involvement = constants.BUILDINVOLVEMENT_SUSPECTED_SOURCECHANGE
                        build.involvements.push(buildInvolvment)
                    }
                }

                await data.updateBuild(build)

            } catch (ex){
                __log.error(`Unexpected error in ${this.constructor.name} : build "${build.id}"`, ex)
            }
        }            
    }
}