const { buildLogsDump } = require("../../helpers/settings")

const BaseDaemon = require(_$+'daemon/base')

/**
 * @extends {BaseDaemon}
 */
module.exports = class BuildLogProcess extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){

        // try to map map local users to users in vcs for a given build
        const pluginsManager = require(_$+'helpers/pluginsManager'),
            constants = require(_$+ 'types/constants'),
            data = await pluginsManager.getExclusive('dataProvider'),
            logHelper = require(_$+'helpers/log'),
            builds = await data.getBuildsWithUnprocessedLogs()

        if (builds.length)
            __log.debug(`found ${builds.length} builds with unprocessed logs`)

        for (const build of builds){
            try {
                const job = await data.getJob(build.jobId, { expected : true })
                if (!job.logParser)
                    continue

                build.logData = await logHelper.parseFromBuild(build, job.logParser)
                build.logStatus = constants.BUILDLOGSTATUS_PROCESSED
                await data.updateBuild(build)
                

            } catch(ex){
                __log.error(`Unexpected error in ${this.constructor.name} : build "${build.id}:${build.build}"`, ex)
            }
        }

    }
}