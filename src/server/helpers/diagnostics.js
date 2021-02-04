const httputils = require('madscience-httputils'),
    urljoin = require('urljoin'),
    pluginsManager = require(_$+'helpers/pluginsManager')


module.exports = {
    /**
     * verifies all the things
     */
    async run(){
        
        const data = await pluginsManager.getExclusive('dataProvider'),
            ciservers = await data.getAllCIServers(),
            jobs = await data.getAllJobs()
        
        // verify plugin config
        let plugins = pluginsManager.getAll()
        __log.info(`${plugins.length} active plugins found`)

        for (let plugin of plugins)
            await plugin.validateSettings()
        

        // verify ciserver urls
        for(let ciserver of ciservers){
            try {
                const url = ciserver.getUrl()
                await httputils.downloadString(url)
                __log.info(`ciserver ${ciserver.name} verified`)
                
            } catch (ex){
                __log.warn(`ciserver ${ciserver.name} remote check failed - ${ex}`)
            }
        }

        //verify job urls
        for(let job of jobs){
            try {

                __log.debug(`verfiying job ${job.name}...`)
                const ciServer = await data.getCIServer(job.CIServerId)
                if (!ciServer){
                    __log.warn(`ERROR : CIServer ${job.CIServerId} defined in job ${job.id} not found`)
                    continue
                }
                
                const url = encodeURI( urljoin(await ciServer.getUrl(), ciServer.name))
                try {
                    await httputils.downloadString(url)
                    __log.debug(`job ${job.name} verified`)
                } catch(ex) {
                    __log.warn(`job ${job.name} was not found at url ${url}: ${ex}`)
                }
                
            } catch (ex){
                __log.error(`job ${job.name} check failed - ${ex}`)
            }
        }        
    }
}