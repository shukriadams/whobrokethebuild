// add daemon workers here
let daemonTypes = [
        'daemon/workers/alertBuildBreaker',
        'daemon/workers/alertBuildStatusChange',
        'daemon/workers/buildDeltaCalculator',
        'daemon/workers/buildImporter',
        'daemon/workers/buildLogParser',
        'daemon/workers/mapUsersToRevisions',
        'daemon/workers/mapRevisions'
    ],
    isRunning = false,
    daemonInstances = {}

module.exports = {

    startAll(){
        const settings = require(_$+'helpers/settings')

        for (const typePath of daemonTypes){
            if (daemonInstances[typePath])
                continue
            
            const Type = require(_$+typePath)
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