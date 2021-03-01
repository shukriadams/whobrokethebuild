const BaseDaemon = require(_$+'daemon/base')

/**
 * @extends {BaseDaemon}
 */
module.exports = class AlertBuildStatusChange extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){
        const pluginsManager = require(_$+'helpers/pluginsManager'),
            constants = require(_$+'types/constants'),
            data = await pluginsManager.getExclusive('dataProvider'),
            jobs = await data.getAllJobs()

        for (const job of jobs){
            try {
                const latestBuild = await data.getLatestBuild(job.id)

                if (!latestBuild || !latestBuild.ended)
                    continue
    
                if ((latestBuild.status === constants.BUILDSTATUS_PASSED && !job.isPassing) ||
                    (latestBuild.status === constants.BUILDSTATUS_FAILED && job.isPassing)){
                        for (const jobContactMethodKey in job.contactMethods){
                            // todo : log that require plugin not found
                            const plugin = await pluginsManager.get(jobContactMethodKey)
                            if (!plugin) 
                                continue
                            
                            // send alert to channel / public forum etc
                            await plugin.alertGroup(job.contactMethods[jobContactMethodKey], job, latestBuild)
                        }
    
                        job.isPassing = latestBuild.status === constants.BUILDSTATUS_PASSED
                        await data.updateJob(job)
                    }

            } catch (ex){
                __log.error(`Unexpected error in ${this.constructor.name} : job "${job.id}:${job.name}"`, ex)
            }
        }                    
    }
}