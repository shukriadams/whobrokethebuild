const 
    httputils = require('madscience-httputils'),
    urljoin = require('urljoin'),
    colors = require('colors/safe'),
    pluginsManager = require(_$+'helpers/pluginsManager')


module.exports = {
    /**
     * verifies all the things
     */
    async run(){
        
        const 
            data = await pluginsManager.getExclusive('dataProvider'),
            ciservers = await data.getAllCIServers(),
            jobs = await data.getAllJobs()
        
        // verify plugin config
        let plugins = pluginsManager.getAll()
        console.log(`${plugins.length} active plugins found`)
        for (let plugin of plugins){

            await plugin.validateSettings()
            console.log(`${plugin.getTypeCode()} passed`)
        }

        // verify ciserver urls
        for(let ciserver of ciservers){
            try {

                console.log(colors.yellow(`verfiying ciserver ${ciserver.name}...`))
                await httputils.downloadString(ciserver.url)
                console.log(colors.green(`ciserver ${ciserver.name} verified`))
                
            } catch (ex){

                console.log(colors.red(`ciserver ${ciserver.name} check failed - ${ex}`))

            }
        }

        //verify job urls
        for(let job of jobs){
            try {

                console.log(colors.yellow(`verfiying job ${job.name}...`))
                const ciServer = await data.getCIServer(job.CIServerId)
                if (!ciServer){
                    console.error(`ERROR : CIServer ${job.CIServerId} defined in job ${job.id} not found`)
                    continue
                }
                const url = encodeURI( urljoin(ciServer.url, ciServer.name))
                try {
                    await httputils.downloadString(url)
                    console.log(colors.green(`job ${job.name} verified`))
                } catch(ex) {
                    console.error(colors.red(`job ${job.name} was not found at url ${url}: ${ex}`))
                }
                
            } catch (ex){

                console.log(colors.red(`job ${job.name} check failed - ${ex}`))

            }
        }        
    }
}