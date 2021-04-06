// add daemon workers here
let isRunning = false,
    daemonInstances = {}

module.exports = {

    initialize(){
        let settings = require(_$+'helpers/settings'),
            glob = require('glob'),
            path = require('path'),
            fsUtils = require('madscience-fsUtils'),
            pluginsManager = require(_$+'helpers/pluginsManager'),
            pluginRoot = pluginsManager.getPluginRootPath(), 
            // get internal daemons
            daemonFiles = glob.sync(`${_$}daemon/workers/*.js`)

        daemonFiles = daemonFiles.concat(glob.sync(`${pluginRoot}/**/daemon.js`, { ignore : ['**/node_modules/**', '**/mock/**']}))
        daemonFiles = daemonFiles.map(daemonFile => fsUtils.fullPathWithoutExtension (daemonFile))

        const blackList = settings.daemonBlacklist ? settings.daemonBlacklist.split(',').filter(b => !!b.length) : [],
            pluginWhitelist = settings.daemonWhitelist ? settings.daemonWhitelist.split(',').filter(b => !!b.length) : []

        for (const typePath of daemonFiles){
            if (daemonInstances[typePath])
                continue

            let skip = false
            for (const blacklisted of blackList)
                if (typePath.endsWith(blacklisted)){
                    __log.info(`Skipping blacklisted daemon "${typePath}" based on mask "${blacklisted}"`)
                    skip = true
                    break
                }
            
            if (skip)
                continue

            if (pluginWhitelist.length){
                // warning : assumes daemon.js is in root of plugin folder
                let include = true
                if (typePath.includes(pluginRoot)){
                    const pluginName = path.basename(path.dirname(typePath))
                    include = pluginWhitelist.includes(pluginName)
                } else {
                    include = false
                    for (const whitelisted of pluginWhitelist)
                        if (typePath.endsWith(whitelisted))
                            include = true
                }

                if (!include){
                    __log.info(`Skipping plugin daemon "${typePath}", not on whitelist`)
                    continue
                }
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