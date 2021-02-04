const BaseDaemon = require(_$+'daemon/base')

module.exports = class extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){

        __log.debug(`buildLogParser daemon doing work ....`)

        const pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            unprocessedBuilds = await data.getBuildsWithUnparsedLogs()

        for (const build of unprocessedBuilds){
            try {
                // try to parse build log for all builds
                const job = await data.getJob(build.jobId, { expected : true }),
                    logParser = job.logParser ? await pluginsManager.get(job.logParser) : null
                
                // if no logparser is selected for job, ignore this build
                if (!logParser)
                    continue

                build.logParsed = logParser.parseErrors(build.log)
                build.isLogParsed = true
                await data.updateBuild(build)
                
                __log.debug(`parsed log for build ${build.id}`)

            } catch(ex){
                __log.error(`Unexpected error in buildLogParser : build "${build.id}"`, ex)
            }
        }
                   
    }
}