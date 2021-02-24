const BaseDaemon = require(_$+'daemon/base')

/**
 * @extends {BaseDaemon}
 */
module.exports = class AlertBuildBreaker extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){
        const constants = require(_$+'types/constants'),
            pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            jobs = await data.getAllJobs()

        for (let job of jobs){
            try {
                const breakingBuild = await data.getBuildThatBrokeJob(job.id) 
                if (!breakingBuild)
                    continue
                
                if (breakingBuild.logStatus === constants.BUILDLOGSTATUS_NOT_FETCHED || breakingBuild.logStatus === constants.BUILDLOGSTATUS_UNPROCESSED){
                    __log.debug(`${job.name} has not been log processed, waiting ....`)
                    continue
                }

                if (!!breakingBuild.involvements.find(r => r.revisionObject === null)){
                    __log.debug(`${job.name} has not been revision mapped yet, waiting ....`)
                    continue
                }

                // go through lineup of usual suspects
                for (const buildInvolvement of breakingBuild.involvements){
                    // prints came back negative, don't worry we'll get them next time
                    if (!buildInvolvement.userId)
                        continue
                    
                    // solid lead, but our snitch gave us a bogus address
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

                        // inform offender they've been caught red-handed
                        await plugin.alertUser(user, breakingBuild)
                    }
                }
            } catch (ex) {
                __log.error(`Unexpected error in ${this.constructor.name} : job "${job.id}"`, ex)
            }

        }
    }
}