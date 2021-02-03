const BaseDaemon = require(_$+'daemon/base')

module.exports = class extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){

        __log.debug(`buildImporter daemon doing work ....`)

        const pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            jobs = await data.getAllJobs()

        // import latest builds
        for (const job of jobs){
            try {
                const ciserver = await data.getCIServer(job.CIServerId, {expected : true}),
                    ciServerPlugin = await pluginsManager.get(ciserver.type)

                __log.debug(`importing builds for job "${job.name}" from ciserver "${ciserver.name}"`)

                await ciServerPlugin.importBuildsForJob(job.id)
            } catch (ex){
                __log.error(`Unexpected error trying to retrieve builds for job "${job.id}"`, ex)
            }
        }
    }
}