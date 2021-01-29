const constants = require(_$+'types/constants'),
    fs = require('fs-extra'),
    pguid = require('pguid'),
    path = require('path'),
    fsUtils = require('madscience-fsUtils'),
    settings = require(_$+'helpers/settings')

async function loadById(table, id, options = {}){
    let recordPath = path.join(settings.dataFolder, 'internaldata', table, `${id}.json`),
        data

    if (await fs.exists(recordPath))
        data = await fs.readJson(recordPath)

    if (!data && options.expected)
        throw `Expected record id ${id} from table "${table}" not found`

    return data
}

async function loadByInternalProperty(table, field, value, options){
    let data = null,
        dataFolder = path.join(settings.dataFolder, 'internaldata', table)
    
    if (await fs.exists(dataFolder)){
        let records = await fsUtils.readFilesInDir(dataFolder)
        for (const recordPath of records){
            const record = await fs.readJson(recordPath)
            if (record[field] === value){
                data = record
                break
            }
        }
    }

    if (!data && options.expected)
        throw `Expected record id ${id} from table "${table}" not found`
    
    return data
}

async function writeRecord(table, record){
    record.id = record.id || pguid()
    const writePath = path.join(settings.dataFolder, 'internaldata', table)
    await fs.ensureDir(writePath)
    await fs.writeJson(path.join(writePath, `${record.id}.json`), record)
    return record
}

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
    getByPublicId : async (publicId, options)=>{
        return await loadByInternalProperty(constants.TABLENAME_USERS, 'publicId', publicId,  options)
    },

    getAllUsers : async () => {
        return []
    },

    removeUser : async id => {

    },

    insertUser : async user =>{
        return await writeRecord(constants.TABLENAME_USERS, user)
    },
    
    updateUser : async user => {
        return await writeRecord(constants.TABLENAME_USERS, user)
    },

    getUser : async (id, options = {}) => {
        return await loadById(constants.TABLENAME_USERS, id, options)
    },

    getUserByExternalName : async (VCServerId, externalName) => {
        return null
    },

    /****************************************************
     * SESSION
     ****************************************************/
    insertSession : async session =>{
        return await writeRecord(constants.TABLENAME_SESSIONS, session)
    },

    getSession: async (id, options) =>{
        return await loadById(constants.TABLENAME_SESSIONS, id, options)
    },


    /****************************************************
     * CIServer
     ****************************************************/
    insertCIServer : async server => {
        return { id : '' }
    },
    
    getCIServer: async (id, options = {}) => {
        if (options.expected)
            throw `Expected record id ${id} from table "${constants.TABLENAME_CISERVERS}" not found`
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

    getJob : async (id, options = {}) => {
        if (options.expected)
            throw `Expected record id ${id} from table "${constants.TABLENAME_JOBS}" not found`
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
    
    getVCServer: async (id, options = {}) =>{
        if (options.expected)
            throw `Expected record id ${id} from table "${constants.TABLENAME_VCSERVERS}" not found`
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

    getAllBuilds () {
        return []
    },

    getBuild : async (id, options = {}) =>{
        if (options.expected)
            throw `Expected record id ${id} from table "${constants.TABLENAME_BUILDS}" not found`
        return null
    },

    getLatestBuild: async jobId => {
        return null
    },
    

    /**
     * Pages through builds. Note that this is inefficient as it pages in server memory instead of in mongo, but oh yeah,
     * mongo can't page
     */
    pageBuilds : async()=>{
        return { items : [], pages : 0 } 
    },



    /**
     * 
     */
    getBuildByExternalId: async (jobId, build, options = {}) =>{
        if (options.expected)
            throw `Expected record id ${id} from table "${constants.TABLENAME_BUILDS}" not found`
        return null
    },

    updateBuild : async build => {

    },

    removeBuild : async id => {
        
    },

    getBuildsWithUnparsedLogs(){
        return []
    },

    /**
     * Gets finished builds with no delta, but also the last build with a delta to server as delta start
     */
    getBuildsWithNoDelta: async()=>{
        return []
    },

    getPreviousBuild(){
        return null
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
    
    removeBuildInvolvement(){

    },

    updateBuildInvolvement : async record => {
        
    },
    
    getAllBuildInvolvement(){
        return null
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

    getBuildInvolvementsWithoutRevisionObjects(){
        return []
    },

    /****************************************************
     * Contact log
     ****************************************************/
    insertContactLog : async contactLog => {
        return { id : '' }
    }, 
    
    getContactLog : async (id, options = {}) => {
        if (options.expected)
            throw `Expected record id ${id} from table "${constants.TABLENAME_CONTACTLOGS}" not found`
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

    },

        
    /****************************************************
     * Utility
     ****************************************************/
    clean(){

    }
}