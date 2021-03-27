const BaseDaemon = require(_$+'daemon/base')

/**
 * @extends {BaseDaemon}
 */
module.exports = class AlertBuildStatusChange extends BaseDaemon {

    async _work(){
        const pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            jobs = await data.getAllJobs()

        for (const job of jobs){
            try {

                // if build has never failed, we don't need to be informing that it's passing
                if (!job.lastBreakIncidentId)
                    continue
                        
                const breakingBuild = await data.getBuildThatBrokeJob(job.id)

                for (const jobContactMethodKey in job.contactMethods){
                    // todo : log that require plugin not found
                    const plugin = await pluginsManager.get(jobContactMethodKey)
                    if (!plugin) 
                        continue
                    
                    // send alert to channel / public forum etc
                    if (breakingBuild)
                        await plugin.alertGroupBuildBreaking(job.contactMethods[jobContactMethodKey], job, breakingBuild.id)
                    else 
                        await plugin.alertGroupBuildPassing(job.contactMethods[jobContactMethodKey], job, job.lastBreakIncidentId)
                }

            } catch (ex){
                __log.error(`Unexpected error in ${this.constructor.name} : job "${job.id}:${job.name}"`, ex)
            }
        }                    
    }
}