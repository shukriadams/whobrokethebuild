module.exports = {

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