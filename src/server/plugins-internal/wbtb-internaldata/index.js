const constants = require(_$+'types/constants'),
    fs = require('fs-extra'),
    pguid = require('pguid'),
    path = require('path'),
    fsUtils = require('madscience-fsUtils'),
    settings = require(_$+'helpers/settings')

async function loadById(table, id, options = {}){
    let recordPath = path.join(settings.dataFolder, 'internaldata', table, `${id}.json`),
        data

    if (await fs.pathExists(recordPath))
        data = await fs.readJson(recordPath)

    if (!data && options.expected)
        throw `Expected record id ${id} from table "${table}" not found`

    return data
}

async function loadByInternalProperty(table, field, value, options){
    let data = null,
        dataFolder = path.join(settings.dataFolder, 'internaldata', table)
    
    if (await fs.pathExists(dataFolder)){
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
    
    async initialize(){
        
    },

    async validateSettings(){
        return true
    },


    /****************************************************
     * USERS
     ****************************************************/
    async getByPublicId(publicId, options){
        return await loadByInternalProperty(constants.TABLENAME_USERS, 'publicId', publicId,  options)
    },

    async getAllUsers(){
        return []
    },

    async removeUser(id){

    },

    async insertUser(user){
        return await writeRecord(constants.TABLENAME_USERS, user)
    },
    
    async updateUser(user){
        return await writeRecord(constants.TABLENAME_USERS, user)
    },

    async getUser(id, options = {}){
        return await loadById(constants.TABLENAME_USERS, id, options)
    },

    async getUserByExternalName(VCServerId, externalName){
        return null
    },

    /****************************************************
     * SESSION
     ****************************************************/
    async insertSession(session){
        return await writeRecord(constants.TABLENAME_SESSIONS, session)
    },

    async getSession(id, options){
        return await loadById(constants.TABLENAME_SESSIONS, id, options)
    },

    async removeSession(id){

    },


    /****************************************************
     * CIServer
     ****************************************************/
    async insertCIServer(server){
        return { id : '' }
    },
    
    async getCIServer(id, options = {}){
        if (options.expected)
            throw `Expected record id ${id} from table "${constants.TABLENAME_CISERVERS}" not found`
        return null
    },
    
    async getAllCIServers(){
        return []
    },

    async updateCIServer(server){
        
    },

    async removeCIServer(id){
        
    },


    /****************************************************
     * JOBS
     ****************************************************/
    async insertJob(job){
        return { id : '' }
    },

    async getJob(id, options = {}){
        if (options.expected)
            throw `Expected record id ${id} from table "${constants.TABLENAME_JOBS}" not found`
        return null
    },

    async getAllJobs(){
        return []
    },

    async getAllJobsByCIServer(ciserverId){
        return []
    },

    async updateJob(job){
        
    },

    async removeJob(id){
        
    },


    /****************************************************
     * VCServer
     ****************************************************/
    async insertVCServer(server){
        return { id : '' }
    },
    
    async getAllVCServers(){
        return []
    },
    
    async getVCServer(id, options = {}){
        if (options.expected)
            throw `Expected record id ${id} from table "${constants.TABLENAME_VCSERVERS}" not found`
        return null
    },

    async updateVCServer(server){
        
    },

    async removeVCServer(id){
        
    },


    /****************************************************
     * Build
     ****************************************************/
    async insertBuild(build){
        return { id : '' }
    },

    getAllBuilds() {
        return []
    },

    async getBuild(id, options = {}){
        if (options.expected)
            throw `Expected record id ${id} from table "${constants.TABLENAME_BUILDS}" not found`
        return null
    },

    async getLatestBuild(jobId){
        return null
    },
    

    /**
     * Pages through builds. Note that this is inefficient as it pages in server memory instead of in mongo, but oh yeah,
     * mongo can't page
     */
    async pageBuilds(){
        return { items : [], pages : 0 } 
    },



    /**
     * 
     */
    async getBuildByExternalId (jobId, build, options = {}){
        if (options.expected)
            throw `Expected record id ${id} from table "${constants.TABLENAME_BUILDS}" not found`
        return null
    },

    async updateBuild(build){

    },

    async removeBuild(id){
        
    },

    getBuildsWithUnparsedLogs(){
        return []
    },

    /**
     * Gets finished builds with no delta, but also the last build with a delta to server as delta start
     */
    async getBuildsWithNoDelta(){
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
    async getCurrentlyBreakingBuild(jobId){
        return null
    },

    /****************************************************
     * Contact log
     ****************************************************/
    async insertContactLog(contactLog){
        return { id : '' }
    }, 
    
    async getContactLog(id, options = {}){
        if (options.expected)
            throw `Expected record id ${id} from table "${constants.TABLENAME_CONTACTLOGS}" not found`
        return null
    },

    async getContactLogByContext(receiverContext, type, eventContext){
        return null
    }, 

    async pageContactLogs(index, pageSize){
        return { items :[], pages : 0}
    },

    async updateContactLog(contactLog){
        
    },

    async clearContactLog(beforeDate){
        
    },

    
    /****************************************************
     * Plugin settings
     ****************************************************/
    async insertPluginSetting(setting){
        return { id : '' }
    }, 

    async updatePluginSetting(setting){

    }, 

    async getPluginSetting(plugin, name){
        return null
    }, 

    async getPluginSettings(plugin){
        return []
    }, 

    async removePluginSettings(plugin){

    },

        
    /****************************************************
     * Utility
     ****************************************************/
    clean(){

    }
}