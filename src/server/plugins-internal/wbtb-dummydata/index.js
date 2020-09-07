/**
 * This module hides our data layer's query logic, and thereby its type. Currently this is
 * build on mongo, but we can replace that with other providers in time.
 */
module.exports = {
    
    initialize : async function(){
        
    },
    

    validateSettings: async () => {
        return true
    },


    /****************************************************
     * USERS
     ****************************************************/
    getByPublicId : async (username, authMethod)=>{
        return null
    },

    getUserById : async id =>{
        return null
    },

    getAllUsers : async () => {
        return []
    },

    removeUser : async id => {

    },

    insertUser : async user =>{
        return { id : ''}
    },
    
    updateUser : async user => {
        
    },

    getUser : async id => {
        return null
    },

    getUserByExternalName : async (VCServerId, externalName) => {
        return null
    },

    /****************************************************
     * SESSION
     ****************************************************/
    insertSession : async session =>{
        return { id : '' }
    },

    getSession: async id =>{
        return null
    },


    /****************************************************
     * CIServer
     ****************************************************/
    insertCIServer : async server => {
        return { id : '' }
    },
    
    getCIServer: async (id, options) => {
        return null
    },
    
    getAllCIServers: async () => {
        return []
    },

    updateCIServer : async server => {
        
    },

    removeCIServer : async id => {
        
    },


    /****************************************************
     * JOBS
     ****************************************************/
    insertJob : async job => {
        return { id : '' }
    },

    getJob : async id => {
        return null
    },

    getAllJobs : async () => {
        return []
    },

    getAllJobsByCIServer : async (ciserverId)=>{
        return []
    },

    updateJob : async job => {
        
    },

    removeJob : async id => {
        
    },


    /****************************************************
     * VCServer
     ****************************************************/
    insertVCServer : async server => {
        return { id : '' }
    },
    
    getAllVCServers: async () => {
        return []
    },
    
    getVCServer: async (id, options) =>{
        return null
    },

    updateVCServer : async server => {
        
    },

    removeVCServer : async id => {
        
    },


    /****************************************************
     * Build
     ****************************************************/
    insertBuild : async build => {
        return { id : '' }
    },

    getBuild : async (id, options) =>{
        return null
    },

    getLatestBuild: async jobId => {
        return null
    },
    

    /**
     * Pages through builds. Note that this is inefficient as it pages in server memory instead of in mongo, but oh yeah,
     * mongo can't page
     */
    pageBuilds : async(jobId, index, pageSize)=>{
        return { items : [], pages : 0 } 
    },



    /**
     * 
     */
    getBuildByExternalId: async (jobId, build) =>{
        return null
    },

    updateBuild : async build => {

    },

    removeBuild : async id => {
        
    },

    /**
     * Gets finished builds with no delta, but also the last build with a delta to server as delta start
     */
    getBuildsWithNoDelta: async()=>{
        return []
    },

    /**
     * Gets the build that broke a job. Returns null if the job is not broken or has never run.
     * 
     * This works by finding the last known passing build in the job, and then takes the earliest _subsequent_ build which
     * failed.
    */
    getCurrentlyBreakingBuild : async jobId =>{
        return null
    },

     /****************************************************
     * Build Involvement
     ****************************************************/
    getUnmappedBuildInvolvements : async()=> {
        return []
    },

    insertBuildInvolvement : async record => {
        return { id : '' }
    },

    updateBuildInvolvement : async record => {
        
    },
    
    getBuildInvolvementByRevision : async (buildId, externalUsername)=>{
        return null
    },

    /**
     * Gets builds that a giver user has been mapped to
     */
    getBuildInvolvementByUserId : async userId =>{
        return null
    },

    getBuildInvolementsByBuild: async (buildId)=>{
        return []
    },


    /****************************************************
     * Contact log
     ****************************************************/
    insertContactLog : async contactLog => {
        return { id : '' }
    }, 
    
    getContactLog : async id => {
        return null
    },

    getContactLogByContext : async (receiverContext, type, eventContext) => {
        return null
    }, 

    pageContactLogs : async(index, pageSize)=>{
        
        return { items :[], pages : 0}
    },

    updateContactLog : async contactLog => {
        
    },

    clearContactLog : async beforeDate => {
        
    },

    
    /****************************************************
     * Plugin settings
     ****************************************************/
    insertPluginSetting : async setting => {
        return { id : '' }
    }, 

    updatePluginSetting : async setting => {

    }, 

    getPluginSetting : async (plugin, name) => {
        return null
    }, 

    getPluginSettings : async (plugin) => {
        return []
    }, 

    removePluginSettings : async plugin => {

    }
}