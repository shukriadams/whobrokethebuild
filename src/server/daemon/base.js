module.exports = class {
    
    constructor(cronMask){

        const CronJob = require('cron').CronJob

        this._isRunning = false
        this._daemon = null
        this._busy = false
        this._daemon = new CronJob(cronMask, async ()=>{
                
            if (this._busy)
                return

            this._busy = true

            try {
                await this._work()

            } catch (ex) {

                __log.error(ex)

            } finally {

                this._busy = false

            }
        }, 
        null,  // oncomplete
        false, 
        null, 
        null, 
        true /* runonitit */ )
    }

    /**
     * Override this with daemon work logic
     */
    async _work(){  }

    isRunning(){
        return this._isRunning
    }

    start(){
        this._daemon.start()
        this._isRunning = true
    }

    stop(){
        this._daemon.stop()
        this.isRunning = false
    }
}