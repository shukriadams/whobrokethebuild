// add daemon workers here
let isRunning = false,
    daemonInstances = {}

module.exports = {

    startAll(){
        let settings = require(_$+'helpers/settings'),
            glob = require('glob'),
            fsUtils = require('madscience-fsutils'),
            pluginsManager = require(_$+'helpers/pluginsManager'),
            pluginRoot = pluginsManager.getPluginRootPath(), 
            // get internal daemons
            daemonFiles = glob.sync(`${_$}daemon/workers/*.js`)

        daemonFiles = daemonFiles.concat(glob.sync(`${pluginRoot}/**/daemon.js`, { ignore : ['**/node_modules/**', '**/mock/**']}))
        daemonFiles = daemonFiles.map(daemonFile => fsUtils.fullPathWithoutExtension (daemonFile))

        for (const typePath of daemonFiles){
            if (daemonInstances[typePath])
                continue
            
            const Type = require(typePath)
            daemonInstances[typePath] = new Type(settings.daemonInterval)
        }

        for (const p in daemonInstances)
            daemonInstances[p].start()

        isRunning = true
    },

    stopAll(){
        for (const p in daemonInstances)
            daemonInstances[p].stop()
        
        isRunning = false
    },

    isRunning(){
        return isRunning
    }

}