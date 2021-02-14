const BaseDaemon = require(_$+'daemon/base')

/**
 * @extends {BaseDaemon}
 */
module.exports = class AlertBuildBreaker extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){
        const pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            jobs = await data.getAllJobs()

        for (let job of jobs){
            try {
                const breakingBuild = await data.getCurrentlyBreakingBuild(job.id) 
                if (!breakingBuild)
                    continue
    
                // inform culprits they've been caught red-handed
                const buildInvolvements = await data.getBuildInvolementsByBuild(breakingBuild.id)
                for (const buildInvolvement of buildInvolvements){
                    // no local user found for build, don't worry we'll get them next time
                    if (!buildInvolvement.userId)
                        continue
                    
                    const user = await data.getUser(buildInvolvement.userId)
                    if (!user){
                        __log.warn(`WARNING - expected user ${buildInvolvement.userId} in buildInvolvement ${buildInvolvement.id} not found`)
                        continue
                    }
    
                    for (const pluginName in user.pluginSettings){
                        const plugin = await pluginsManager.get(pluginName)
                        if (!plugin){
                            __log.warn(`WARNING - expected plugin ${pluginName} not found`)
                            continue
                        }

                        if (plugin.__wbtb.category !== 'contactMethod')
                            continue

                        await plugin.alertUser(user, breakingBuild)
                    }
                }
            } catch (ex) {
                __log.error(`Unexpected error in alertBuildBreaker : job "${job.id}"`, ex)
            }

        }
    }
}