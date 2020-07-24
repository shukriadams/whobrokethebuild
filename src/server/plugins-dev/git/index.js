module.exports = {

    // required by plugin interface
    getDescription(){
        return {
            id : 'git',
            name : null
        }
    },

    async validateSettings(){
        return true
    },

    async getRevision(revision){
        return null
    }
}