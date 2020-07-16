module.exports = {
    
    getTypeCode(){
        return 'perforce'
    },

    validateSettings: async () => {
        return true
    },
    
    // required by plugin interface
    getDescription(){
        return {
            id : 'perforce',
            name : null
        }
    }
    

}