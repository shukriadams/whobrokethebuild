// add daemon workers here
let isRunning = false,
    daemonInstances = {}

module.exports = {

    startAll(){
        let settings = require(_$+'helpers/settings'),
            glob = require('glob'),
            fsUtils = require('madscience-fsUtils'),
            pluginsManager = require(_$+'helpers/pluginsManager'),
            pluginRoot = pluginsManager.getPluginRootPath(), 
            // get internal daemons
            daemonFiles = glob.sync(`${_$}daemon/workers/*.js`)

        daemonFiles = daemonFiles.concat(glob.sync(`${pluginRoot}/**/daemon.js`, { ignore : ['**/node_modules/**', '**/mock/**']}))
        daemonFiles = daemonFiles.map(daemonFile => fsUtils.fullPathWithoutExtension (daemonFile))

        const blackList = (settings.daemonBlacklist || '').split(',').filter(b => !!b.length)
        for (const typePath of daemonFiles){
            if (daemonInstances[typePath])
                continue
            
            for (const blacklisted of blackList)
                if (typePath.endsWith(blacklisted)){
                    __log.info(`SKipping blacklisted daemon "${typePath}" based on mask "${blacklisted}"`)
                    continue
                }

            const Type = require(typePath)
            daemonInstances[typePath] = new Type(settings.daemonInterval)
            __log.info(`Loading daemon ${typePath}`)
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