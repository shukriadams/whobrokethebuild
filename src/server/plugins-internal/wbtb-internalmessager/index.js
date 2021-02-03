module.exports = {
    
    /**
     * required by plugin interface
     */
    async validateSettings() {
        return true
    },


    /**
     * Required by all "contact" plugins
     */
    async canTransmit(){
        return true
    },

    /**
     * Required by all "contact" plugins
     */
    async alertGroup(slackContactMethod, job, build, delta){

    },

    /**
     * Required by all "contact" plugins
     */
    async alertUser(user, build){

    }
}