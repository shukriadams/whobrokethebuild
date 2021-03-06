const BaseDaemon = require(_$+'daemon/base')

/**
 * @extends {BaseDaemon}
 */
module.exports = class BuildImporter extends BaseDaemon {

    async _work(){
        const pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            jobs = await data.getAllJobs()

        // import latest builds
        for (const job of jobs){
            try {
                const ciserver = await data.getCIServer(job.CIServerId, {expected : true}),
                    ciServerPlugin = await pluginsManager.get(ciserver.type)

                await ciServerPlugin.importBuildsForJob(job)
            } catch (ex){
                __log.error(`Unexpected error in ${this.constructor.name} : job "${job.id}:${job.name}"`, ex)
            }
        }
    }
}