const BaseDaemon = require(_$+'daemon/base')

module.exports = class extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){

        __log.debug(`alertBuildBreaker daemon doing work ....`)

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
                        __log.info(`WARNING - expected user ${buildInvolvement.userId} in buildInvolvement ${buildInvolvement.id} not found`)
                        continue
                    }
    
                    for (const contactMethod in user.contactMethods){
                        const contactPlugin = await pluginsManager.get(contactMethod)
                        if (!contactPlugin){
                            __log.info(`WARNING - expected plugin ${contactMethod.type} not found`)
                            continue
                        }
                        
                        await contactPlugin.alertUser(user, breakingBuild)
                    }
                }
            } catch (ex) {
                __log.error(`Unexpected error trying to alert user of build break in job "${job.id}"`, ex)
            }

        }
    }
}