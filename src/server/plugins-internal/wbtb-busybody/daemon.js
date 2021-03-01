const BaseDaemon = require(_$+'daemon/base'),
    thisType = 'wbtb-busybody'

/**
 * @extends {BaseDaemon}
 */
module.exports = class BusyBodyDaemon extends BaseDaemon {

    constructor(...args){
        super(...args)
    }
    
    async _work(){
        let pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider'),
            jobs = await data.getAllJobs(),
            busybodies = await data.getUsersUsingPlugin(thisType)

        // filter for everyone who wants to be informed of all build errors
        busybodies = busybodies.filter(busyBody => !!busyBody.pluginSettings[thisType].alertOnAllBuildErrors)

        for (let job of jobs){
            try {
                const breakingBuild = await data.getBuildThatBrokeJob(job.id) 
                // job is currently not broken, ignore
                if (!breakingBuild)
                    continue
           
                for (const busybody of busybodies){
                    const contactPlugins = pluginsManager.getUserPluginsByCategory(busybody, 'contactMethod')

                    if (!contactPlugins.length)
                        __log.warn(`User ${busybody.name} has requested to be alerted on all build errors, but has no contact method`)

                    for (const contactPlugin of contactPlugins)
                        await contactPlugin.alertUser(busybody, breakingBuild, null, 'interested')
                }
            } catch (ex) {
                __log.error(`Unexpected error in ${this.constructor.name} : job "${job.id}"`, ex)
            }
        }
    }
}