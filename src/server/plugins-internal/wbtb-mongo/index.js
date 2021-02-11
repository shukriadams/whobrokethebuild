/**
 * This module hides our data layer's query logic, and thereby its type. Currently this is
 * build on mongo, but we can replace that with other providers in time.
 */
const constants = require(_$+'types/constants'),
    ObjectID = require('mongodb').ObjectID,
    settings = require(_$+'helpers/settings'),
    CIServer = require(_$+'types/CIServer'),
    _mongo = require('./mongo'),
    _normalize = (input, normalizer)=>{
        if (Array.isArray(input)){
            const out = []
            for(let r of input)
                out.push(normalizer(r))
            return out
        } else{
            if (!input)
                return null
            return normalizer(input)
        }
    },
    _arrayToNormalizedPage = (items, index, pageSize, normalizer)=>{
        let pages = Math.floor(items.length / pageSize)
        if (items.length % pageSize)
            pages ++

        items = _normalize(items.slice(index * pageSize, (index * pageSize) + pageSize), normalizer)
        return { items, pages} 
    },    
    _normalizeJob = job =>{
        job.id = job._id.toString()
        job.CIServerId = job.CIServerId.toString()
        job.VCServerId = job.VCServerId.toString()
        delete job._id
        return job
    },
    _denormalizeJob = record =>{
        const job = Object.assign({}, record)
        if (job.id){
            job._id = new ObjectID(job.id)
            delete job.id
        }

        job.CIServerId = new ObjectID(job.CIServerId)
        job.VCServerId = new ObjectID(job.VCServerId)
        return job
    },
    _normalizeBuild = build =>{
        build.id = build._id.toString()
        build.jobId = build.jobId.toString()
        delete build._id
        return build
    }, 
    _denormalizeBuild = record =>{
        const build = Object.assign({}, record)
        if (build.id){
            build._id = new ObjectID(build.id)
            delete build.id
        }

        build.jobId = new ObjectID(build.jobId)
        return build
    },        
    _normalizeBuildInvolvement = buildInvolvement =>{
        buildInvolvement.id = buildInvolvement._id.toString()
        buildInvolvement.buildId = buildInvolvement.buildId.toString()
        if (buildInvolvement.userId)
            buildInvolvement.userId = buildInvolvement.userId.toString()
        delete buildInvolvement._id
        return buildInvolvement
    },
    _denormalizeBuildInvolvement = record =>{
        const buildInvolvement = Object.assign({}, record)
        if (buildInvolvement.id){
            buildInvolvement._id = new ObjectID(buildInvolvement.id)
            delete buildInvolvement.id
        }        
        buildInvolvement.buildId = new ObjectID(buildInvolvement.buildId)
        if (buildInvolvement.userId)
            buildInvolvement.userId = new ObjectID(buildInvolvement.userId)
        return buildInvolvement
    },
    _normalizeVCServer = server =>{
        server.id = server._id.toString()
        delete server._id
        return server
    },
    _denormalizeVCServer = record =>{
        const server = Object.assign({}, record)
        if (server.id){
            server._id = new ObjectID(server.id)
            delete server.id
        }         
        return server
    },     
    _normalizeCIServer = rawRecord =>{
        const record = Object.assign( new CIServer(), rawRecord)
        record.id = record._id.toString()
        delete record._id
        return record
    },
    _denormalizeCIServer = record =>{
        const server = Object.assign({}, record)
        if (server.id){
            server._id = new ObjectID(server.id)
            delete server.id
        }         
        return server
    },
    _normalizeUsers = user =>{
        user.id = user._id.toString()

        for (let mapping of user.userMappings)
            mapping.VCServerId = mapping.VCServerId.toString()

        delete user._id
        return user
    },
    _denormalizeUsers = record =>{
        const user = Object.assign({}, record)
        if (user.id){
            user._id = new ObjectID(user.id)
            delete user.id
        }         

        for (let mapping of user.userMappings)
            mapping.VCServerId = new ObjectID(mapping.VCServerId)

        return user
    },    
    _normalizeSession = session =>{
        session.id = session._id.toString()
        session.userId = session.userId.toString()
        delete session._id
        return session
    },
    _denormalizeSession = record =>{
        const session = Object.assign({}, record)
        if (session.id){
            session._id = new ObjectID(session.id)
            delete session.id
        }
        session.userId = new ObjectID(session.userId)         
        return session
    },
    _normalizeContactLog = contactLog =>{
        contactLog.id = contactLog._id.toString()
        delete contactLog._id
        return contactLog
    },
    _denormalizeContactLog = record =>{
        const contactLog = Object.assign({}, record)
        if (contactLog.id){
            contactLog._id = new ObjectID(contactLog.id)
            delete contactLog.id
        }
        return contactLog
    },
    _normalizePluginSetting = setting =>{
        setting.id = setting._id.toString()
        delete setting._id
        return setting
    },
    _denormalizePluginSetting = record =>{
        const setting = Object.assign({}, record)
        if (setting.id){
            setting._id = new ObjectID(setting.id)
            delete setting.id
        }         
        return setting
    }

module.exports = {
    
    _mongo,

    async initialize (){
        await _mongo.initialize()
    },


    async validateSettings() {
        if (!settings.mongoConnectionString){
            __log.error(`mongo plugin requires "mongoConnectionString" with format "mongodb://USER:PASSWORD@IP:PORT"`)
            return false
        }
        
        if (!settings.mongoDBName){
            __log.error(`mongo plugin requires "mongoDBName" with name of database to use`)
            return false
        }

        return true
    },


    /****************************************************
     * USERS
     ****************************************************/
    async getByPublicId (username, authMethod){
        return _normalize(await _mongo.findFirst(constants.TABLENAME_USERS, {
            $and: [ 
                { 'authMethod' :{ $eq : authMethod } },
                { 'publicId' :{ $eq : username } }
            ]
        }), _normalizeUsers)
    },

    async getAllUsers () {
        return _normalize( await _mongo.find(constants.TABLENAME_USERS, { }), _normalizeUsers)
    },

    async removeUser(id) {
        await _mongo.remove(constants.TABLENAME_USERS, { 
            _id : new ObjectID(id) 
        })
    },

    async insertUser (user){
        return _normalize(await _mongo.insert(constants.TABLENAME_USERS, _denormalizeUsers(user)), _normalizeUsers)
    },
    
    async updateUser(user) {
        await _mongo.update(constants.TABLENAME_USERS, _denormalizeUsers(user))
    },

    async getUser (id) {
        return _normalize(await _mongo.getById(constants.TABLENAME_USERS, id), _normalizeUsers)
    },

    async getUserByExternalName (VCServerId, externalName) {
        return _normalize(await _mongo.findFirst(constants.TABLENAME_USERS, {
            $and: [ 
                { 'userMappings.name' : { $eq : externalName } },
                { 'userMappings.VCServerId' : { $eq : new ObjectID(VCServerId) } }
            ]
        }), _normalizeUsers)
    },

    /****************************************************
     * SESSION
     ****************************************************/
    async insertSession (session){
        return _normalize(await _mongo.insert(constants.TABLENAME_SESSIONS, _denormalizeSession(session)), _normalizeSession)
    },

    async getSession(id) {
        return _normalize(await _mongo.getById(constants.TABLENAME_SESSIONS, id), _normalizeSession)
    },

    async removeSession(id) {
        await _mongo.remove(constants.TABLENAME_SESSIONS, { 
            _id : new ObjectID(id) 
        })
    },

    /****************************************************
     * CIServer
     ****************************************************/
    async insertCIServer (server) {
        return _normalize(await _mongo.insert(constants.TABLENAME_CISERVERS, _denormalizeCIServer(server)), _normalizeCIServer)
    },
    
    async getCIServer (id, options){
        return _normalize(await _mongo.getById(constants.TABLENAME_CISERVERS, id, options), _normalizeCIServer)
    },
    
    async getAllCIServers () {
        return _normalize(await _mongo.find(constants.TABLENAME_CISERVERS, { }), _normalizeCIServer)
    },

    async updateCIServer (server) {
        await _mongo.update(constants.TABLENAME_CISERVERS, _denormalizeCIServer(server))
    },

    async removeCIServer (id) {
        await _mongo.remove(constants.TABLENAME_CISERVERS, { 
            _id : new ObjectID(id) 
        })
    },


    /****************************************************
     * JOBS
     ****************************************************/
    async insertJob (job) {
        return _normalize(await _mongo.insert(constants.TABLENAME_JOBS, _denormalizeJob(job)), _normalizeJob)
    },


    /**
     * @returns {Promise<import('../../types/job').Job>}
     */
    async getJob(id, options) {
        return _normalize(await _mongo.getById(constants.TABLENAME_JOBS, id, options), _normalizeJob)
    },

    async getAllJobs () {
        return _normalize(await _mongo.find(constants.TABLENAME_JOBS, { }), _normalizeJob)
    },

    async getAllJobsByCIServer (ciserverId){
        return _normalize(await _mongo.find(constants.TABLENAME_JOBS, {
            CIServerId : new ObjectID(ciserverId)
         }), _normalizeJob)
    },

    async updateJob (job) {
        await _mongo.update(constants.TABLENAME_JOBS, _denormalizeJob(job))
    },

    async removeJob (id) {
        await _mongo.remove(constants.TABLENAME_JOBS, { 
            _id : new ObjectID(id) 
        })
    },


    /****************************************************
     * VCServer
     ****************************************************/
    async insertVCServer (server) {
        return _normalize(await _mongo.insert(constants.TABLENAME_VCSERVERS, _denormalizeVCServer(server)), _normalizeVCServer)
    },
    
    async getAllVCServers(){
        return _normalize(await _mongo.find(constants.TABLENAME_VCSERVERS, { }), _normalizeVCServer)
    },
    
    async getVCServer (id, options) {
        return _normalize(await _mongo.getById(constants.TABLENAME_VCSERVERS, id, options), _normalizeVCServer)
    },

    async updateVCServer (server) {
        await _mongo.update(constants.TABLENAME_VCSERVERS, _denormalizeVCServer(server))
    },

    async removeVCServer(id) {
        await _mongo.remove(constants.TABLENAME_VCSERVERS, { 
            _id : new ObjectID(id) 
        })
    },


    /****************************************************
     * Build
     ****************************************************/
    async insertBuild(build) {
        return _normalize(await _mongo.insert(constants.TABLENAME_BUILDS, _denormalizeBuild(build)), _normalizeBuild)
    },

    async getAllBuilds () {
        return _normalize(await _mongo.find(constants.TABLENAME_BUILDS, { }), _normalizeBuild)
    },

    async getBuild (id, options) {
        return _normalize(await _mongo.getById(constants.TABLENAME_BUILDS, id, options), _normalizeBuild)
    },

    async getLatestBuild(jobId) {
        return _normalize((await _mongo.aggregate(constants.TABLENAME_BUILDS, 
            {
                $match : {
                    $and: [ 
                        { 'jobId' :{ $eq : new ObjectID(jobId) } }
                    ],
                }
            },
            {
                $sort: { 'started': -1}
            },
            {
                $limit: 1
            }
        )).pop(), _normalizeBuild)
    },
    

    /**
     * Pages through builds. Note that this is inefficient as it pages in server memory instead of in mongo, but oh yeah,
     * mongo can't page
     */
    async pageBuilds (jobId, index, pageSize){
        let items = await _mongo.find(constants.TABLENAME_BUILDS, 
            {
                $and: [ 
                    {'jobId' :{ $eq : new ObjectID(jobId) } }
                ]
            },
            {
                started : -1
            }
        )

        let pages = Math.floor(items.length / pageSize)
        if (items.length % pageSize)
            pages ++

        items = _normalize(items.slice(index * pageSize, (index * pageSize) + pageSize), _normalizeBuild)
        return { items, pages} 
    },



    /**
     * 
     */
    async getBuildByExternalId (jobId, build){
        return _normalize(await _mongo.findFirst(constants.TABLENAME_BUILDS, {
            $and: [ 
                { 'jobId' :{ $eq : new ObjectID(jobId) } },
                { 'build' :{ $eq : build } }
            ]
        }), _normalizeBuild)
    },

    async updateBuild(build) {
        await _mongo.update(constants.TABLENAME_BUILDS, _denormalizeBuild(build))
    },

    async removeBuild(id) {
        // remove children
        await _mongo.remove(constants.TABLENAME_BUILDINVOLVEMENTS, { 
            buildId : new ObjectID(id) 
        })

        // remove record
        await _mongo.remove(constants.TABLENAME_BUILDS, { 
            _id : new ObjectID(id) 
        })
    },

    async removeAllBuilds(){
        // remove children
        await _mongo.remove(constants.TABLENAME_BUILDINVOLVEMENTS)

        // remove record
        await _mongo.remove(constants.TABLENAME_BUILDS)
    },

    /**
     * A build's log must already be fetched (ie, be not null) to qualify
     */
    async getBuildsWithUnparsedLogs(){
        return _normalize(await _mongo.find(constants.TABLENAME_BUILDS, {
            $and: [ 
                { 'isLogParsed' :{ $eq : false } },
                { 'log' :{ $ne : null } }
            ]
        }), _normalizeBuild)
    },

    /**
     * Gets finished builds with no delta
     */
    async getBuildsWithNoDelta(){
        /*
        let lastWithDelta = (await _mongo.aggregate(constants.TABLENAME_BUILDS, 
            {
                $match : {
                    $and: [ 
                        { 'delta' :{ $ne : null } },
                    ]            
                }
            },

            {
                $sort: { 'started': -1 } // sort latest first
            },

            // group by parent build
            {
                $group: {
                    "_id":{
                        "build":"$build"
                    },
                    "build": {$first:"$$ROOT"}
                }
            },

            {
                $limit: 1
            })
        ).pop()
            */
        // return all null deltas, and if exists, last with delta to server as starting history
        return _normalize(await _mongo.aggregate(constants.TABLENAME_BUILDS, 
            {
                $match : {
                    $and: [ 
                        // builds with no delta or last with delta
                        { 'delta' :{ $eq : null } },

                        // finished builds only - either passed or failed
                        {
                            $or : [
                                { 'status' :{ $eq : constants.BUILDSTATUS_FAILED } },
                                { 'status' :{ $eq : constants.BUILDSTATUS_PASSED } }
                            ]
                        }
                    ]            
                }
            }

        ), _normalizeBuild)
    },


    /**
     * Gets the previous build before a given build object.
     */
    async getPreviousBuild(build){
        const previosBuild = (await _mongo.aggregate(constants.TABLENAME_BUILDS, 
            {
                $match : {
                    $and: [ 
                        { 'jobId' :{ $eq : new ObjectID(build.jobId) } },
                        { 'started' :{ $lt : build.started } }
                    ]
                }
            },
        
            {
                // sort latest first
                $sort: { 'started': -1}
            },

            {
                $limit: 1
            }
        )).pop()

        return previosBuild ? _normalize(previosBuild, _normalizeBuild) : null
    },

    /**
     * Gets the build that broke a job. Returns null if the job is not broken or has never run.
     * 
     * This works by finding the last known passing build in the job, and then takes the earliest _subsequent_ build which
     * failed.
    */
   async getCurrentlyBreakingBuild (jobId){
        const lastWorkingBuild = (await _mongo.aggregate(constants.TABLENAME_BUILDS, 
            {
                $match : {
                    $and: [ 
                        { 'jobId' :{ $eq : new ObjectID(jobId) } },
                        { 'status' :{ $eq : constants.BUILDSTATUS_PASSED } }
                    ]
                }
            },
        
            {
                // sort latest first
                $sort: { 'started': -1}
            },

            {
                $limit: 1
            }
        )).pop()

        if (!lastWorkingBuild)
            return null

        const breakingBuild = await _mongo.aggregate(constants.TABLENAME_BUILDS, 
            {
                $match :  {
                    $and: [ 
                        { 'jobId' :{ $eq : new ObjectID(jobId) } },
                        { 'status' :{ $eq : constants.BUILDSTATUS_FAILED } },
                        { 'started' : { $gt : lastWorkingBuild.started } }
                    ]
                }
            },

            {
                // sort earliest first
                $sort: { 'started': 1 }
            },

            {
                $limit: 1
            }
        )

        return _normalize(breakingBuild.length ? breakingBuild[0] : null, _normalizeBuild)
    },


     /****************************************************
     * Build Involvement
     ****************************************************/
    async getUnmappedBuildInvolvements (){
        return _normalize(await _mongo.find(constants.TABLENAME_BUILDINVOLVEMENTS, {
            $and: [ 
                { 'userId' :{ $eq : null } }
            ]
        }), _normalizeBuildInvolvement)
    },

    async insertBuildInvolvement(record) {
        return _normalize(await _mongo.insert(constants.TABLENAME_BUILDINVOLVEMENTS, _denormalizeBuildInvolvement(record)), _normalizeBuildInvolvement)
    },

    async removeBuildInvolvement(id) {
        await _mongo.remove(constants.TABLENAME_BUILDINVOLVEMENTS, { 
            _id : new ObjectID(id) 
        })
    },

    async updateBuildInvolvement(record) {
        await _mongo.update(constants.TABLENAME_BUILDINVOLVEMENTS, _denormalizeBuildInvolvement(record))
    },
    
    async getAllBuildInvolvement () {
        return _normalize(await _mongo.find(constants.TABLENAME_BUILDINVOLVEMENTS, { }), _normalizeBuildInvolvement)
    },

    /** 
     * Gets build involvements for a given build and given revision
     */
    async getBuildInvolvementByRevision (buildId, revision){
        return _normalize(await _mongo.findFirst(constants.TABLENAME_BUILDINVOLVEMENTS, {
            $and: [
                { 'revision' :{ $eq : revision } },
                { 'buildId' :{ $eq : new ObjectID(buildId) } }
            ]
        }), _normalizeBuildInvolvement)
    },

    /**
     * Gets builds that a giver user has been mapped to
     */
    async getBuildInvolvementByUserId (userId){
        const buildInvolvements = await _mongo.aggregate(constants.TABLENAME_BUILDINVOLVEMENTS, 
            {
                "$lookup": {
                    "from": constants.TABLENAME_BUILDS,
                    "localField": "buildId",
                    "foreignField": "_id",
                    "as": "__build"
                }
            },
            {
                $match: { 
                    $and: [ 
                        { "userId" :{ $eq : new ObjectID(userId) } }
                    ] 
                }
            }
        )

        // flatten mongo join collection to single normalized build object
        for (const buildInvolvement of buildInvolvements)
            buildInvolvement.__build = buildInvolvement.__build.length ? _normalizeBuild(buildInvolvement.__build[0]) : null

        return _normalize(buildInvolvements, _normalizeBuildInvolvement)
    },

    async getBuildInvolementsByBuild (buildId){
        return _normalize(await _mongo.find(constants.TABLENAME_BUILDINVOLVEMENTS, {
            $and: [
                { 'buildId' :{ $eq : new ObjectID(buildId) } }
            ]
        }), _normalizeBuildInvolvement)
    },

    async getBuildInvolvementsWithoutRevisionObjects(){
        const buildInvolvements = await _mongo.aggregate(constants.TABLENAME_BUILDINVOLVEMENTS, 
            {
                "$lookup": {
                    "from": constants.TABLENAME_BUILDS,
                    "localField": "buildId",
                    "foreignField": "_id",
                    "as": "__build"
                }
            },
            
            {
                $match: { 
                    $and: [ 
                        { 'revisionObject' :{ $eq : null }},
                        { '__build.log' :{ $ne : null }}
                    ] 
                }
            }
        )

        return _normalize(buildInvolvements, _normalizeBuildInvolvement)

        return _normalize(await _mongo.find(constants.TABLENAME_BUILDINVOLVEMENTS, {
            $and: [
                { 'revisionObject' :{ $eq : null} }
            ]
        }), _normalizeBuildInvolvement)
    },

    /****************************************************
     * Contact log
     ****************************************************/
    async insertContactLog (contactLog){
        return _normalize(await _mongo.insert(constants.TABLENAME_CONTACTLOGS, _denormalizeContactLog(contactLog)), _normalizeContactLog)
    }, 
    
    async getContactLog (id) {
        return _normalize(await _mongo.getById(constants.TABLENAME_CONTACTLOGS, id), _normalizeContactLog)
    },

    async getContactLogByContext (receiverContext, type, eventContext) {
        return _normalize(await _mongo.findFirst(constants.TABLENAME_CONTACTLOGS, {
            $and: [
                { 'receiverContext' :{ $eq : receiverContext } },
                { 'type' :{ $eq : type } },
                { 'eventContext' :{ $eq : eventContext } }
            ]
        }), _normalizeContactLog)
    }, 

    async pageContactLogs (index, pageSize){
        const items = await _mongo.find(constants.TABLENAME_CONTACTLOGS, 
            {

            },

            {
                created : -1
            }
        )
        return _arrayToNormalizedPage(items, index, pageSize, _normalizeContactLog)
    },

    async updateContactLog (contactLog) {
        await _mongo.update(constants.TABLENAME_CONTACTLOGS, _denormalizeContactLog(contactLog))
    },

    async clearContactLog (beforeDate) {
        await _mongo.remove(constants.TABLENAME_CONTACTLOGS, { 
            $and: [
                { 'created' :{ $lt : beforeDate.getTime() } }
            ]
        })
    },

    /****************************************************
     * Plugin settings
     ****************************************************/
    async insertPluginSetting (setting) {
        return _normalize(await _mongo.insert(constants.TABLENAME_PLUGINSETTINGS, _denormalizePluginSetting(setting)), _normalizePluginSetting)
    }, 

    async updatePluginSetting (setting) {
        await _mongo.update(constants.TABLENAME_PLUGINSETTINGS, _denormalizePluginSetting(setting))
    }, 

    async getPluginSetting (plugin, name) {
        return _normalize(await _mongo.findFirst(constants.TABLENAME_PLUGINSETTINGS, {
            $and: [
                { 'plugin' :{ $eq : plugin } },
                { 'name' :{ $eq : name } }
            ]
        }), _normalizePluginSetting)
    }, 

    async getPluginSettings (plugin) {
        return _normalize(await _mongo.findFirst(constants.TABLENAME_PLUGINSETTINGS, {
            $and: [
                { 'plugin' :{ $eq : plugin } }
            ]
        }), _normalizePluginSetting)
    }, 

    async removePluginSettings (plugin) {
        await _mongo.remove(constants.TABLENAME_PLUGINSETTINGS, { 
            $and: [
                { 'plugin' :{ $eq : plugin } }
            ]
        })
    },

    /****************************************************
     * Utility
     ****************************************************/
    async clean () {
        const vcServers = await this.getAllVCServers(),
            ciServers = await this.getAllCIServers(),
            jobs = await this.getAllJobs(),
            builds = await this.getAllBuilds(),
            buildInvolvements = await this.getAllBuildInvolvement()
        
        for (let i = 0; i < jobs.length ; i ++){
            let job = jobs[jobs.length - 1 - i],
                remove = false

            if (!ciServers.find(ciServer => ciServer.id === job.CIServerId))
                remove = true

            if (!vcServers.find(vcServer => vcServer.id === job.VCServerId))
                remove = true

            if (remove){
                await this.removeJob(job.id)
                jobs.splice(i, 1)
                __log.info(`Cleaned out orphan job ${job.id}`)
            }
        }
        
        for (let i = 0 ; i < builds.length ; i ++){
            const build = builds[builds.length - 1 - i]
            if (!jobs.find(job => job.id === build.jobId)){
                await this.removeBuild(build.id)
                builds.splice(i, 1)
                __log.info(`Cleaned out orphan build ${build.id}`)
            }
        }

        for (let i = 0 ; i < buildInvolvements.length ; i ++){
            const buildInvolvement = buildInvolvements[buildInvolvements.length - 1 - i]
            if (!builds.find(build => build.id === buildInvolvement.buildId)){
                await this.removeBuildInvolvement(buildInvolvement.id)
                buildInvolvements.splice(i, 1)
                __log.info(`Cleaned out orphan buildInvolvement ${buildInvolvement.id}`)
            }
        }
    }

}