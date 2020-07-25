/**
 * This module hides our data layer's query logic, and thereby its type. Currently this is
 * build on mongo, but we can replace that with other providers in time.
 */
const 
    constants = require(`${_$}types/constants`),
    ObjectID = require('mongodb').ObjectID,
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

        item = _normalize(items.slice(index * pageSize, (index * pageSize) + pageSize), normalizer)
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
    _normalizeCIServer = server =>{
        server.id = server._id.toString()
        delete server._id
        return server
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

    initialize : async function(){
        await _mongo.initialize()
    },

    
    validateSettings: async () => {
        return true
    },
    


    /****************************************************
     * USERS
     ****************************************************/
    getByPublicId : async (username, authMethod)=>{
        return _normalize(await _mongo.findFirst(constants.TABLENAME_USERS, {
            $and: [ 
                { 'authMethod' :{ $eq : authMethod } },
                { 'publicId' :{ $eq : username } }
            ]
        }), _normalizeUsers)
    },

    getUserById : async id =>{
        return _normalize(await _mongo.getById(constants.TABLENAME_USERS, id), _normalizeUsers)
    },

    getAllUsers : async () => {
        return _normalize( await _mongo.find(constants.TABLENAME_USERS, { }), _normalizeUsers)
    },

    removeUser : async id => {
        await _mongo.remove(constants.TABLENAME_USERS, { 
            _id : new ObjectID(id) 
        })
    },

    insertUser : async user =>{
        return _normalize(await _mongo.insert(constants.TABLENAME_USERS, _denormalizeUsers(user)), _normalizeUsers)
    },
    
    updateUser : async user => {
        await _mongo.update(constants.TABLENAME_USERS, _denormalizeUsers(user))
    },

    getUser : async id => {
        return _normalize(await _mongo.getById(constants.TABLENAME_USERS, id), _normalizeUsers)
    },

    getUserByExternalName : async (VCServerId, externalName) => {
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
    insertSession : async session =>{
        return _normalize(await _mongo.insert(constants.TABLENAME_SESSIONS, _denormalizeSession(session)), _normalizeSession)
    },

    getSession: async id =>{
        return _normalize(await _mongo.getById(constants.TABLENAME_SESSIONS, id), _normalizeSession)
    },


    /****************************************************
     * CIServer
     ****************************************************/
    insertCIServer : async server => {
        return _normalize(await _mongo.insert(constants.TABLENAME_CISERVERS, _denormalizeCIServer(server)), _normalizeCIServer)
    },
    
    getCIServer: async (id, options) => {
        return _normalize(await _mongo.getById(constants.TABLENAME_CISERVERS, id, options), _normalizeCIServer)
    },
    
    getAllCIServers: async () => {
        return _normalize(await _mongo.find(constants.TABLENAME_CISERVERS, { }), _normalizeCIServer)
    },

    updateCIServer : async server => {
        await _mongo.update(constants.TABLENAME_CISERVERS, _denormalizeCIServer(server))
    },

    removeCIServer : async id => {
        await _mongo.remove(constants.TABLENAME_CISERVERS, { 
            _id : new ObjectID(id) 
        })
    },


    /****************************************************
     * JOBS
     ****************************************************/
    insertJob : async job => {
        return _normalize(await _mongo.insert(constants.TABLENAME_JOBS, _denormalizeJob(job)), _normalizeJob)
    },

    getJob : async id => {
        return _normalize(await _mongo.getById(constants.TABLENAME_JOBS, id), _normalizeJob)
    },

    getAllJobs : async () => {
        return _normalize(await _mongo.find(constants.TABLENAME_JOBS, { }), _normalizeJob)
    },

    getAllJobsByCIServer : async (ciserverId)=>{
        return _normalize(await _mongo.find(constants.TABLENAME_JOBS, {
            CIServerId : new ObjectID(ciserverId)
         }), _normalizeJob)
    },

    updateJob : async job => {
        await _mongo.update(constants.TABLENAME_JOBS, _denormalizeJob(job))
    },

    removeJob : async id => {
        await _mongo.remove(constants.TABLENAME_JOBS, { 
            _id : new ObjectID(id) 
        })
    },


    /****************************************************
     * VCServer
     ****************************************************/
    insertVCServer : async server => {
        return _normalize(await _mongo.insert(constants.TABLENAME_VCSERVERS, _denormalizeVCServer(server)), _normalizeVCServer)
    },
    
    getAllVCServers: async () => {
        return _normalize(await _mongo.find(constants.TABLENAME_VCSERVERS, { }), _normalizeVCServer)
    },
    
    getVCServer: async (id, options) =>{
        return _normalize(await _mongo.getById(constants.TABLENAME_VCSERVERS, id, options), _normalizeVCServer)
    },

    updateVCServer : async server => {
        await _mongo.update(constants.TABLENAME_VCSERVERS, _denormalizeVCServer(server))
    },

    removeVCServer : async id => {
        await _mongo.remove(constants.TABLENAME_VCSERVERS, { 
            _id : new ObjectID(id) 
        })
    },


    /****************************************************
     * Build
     ****************************************************/
    insertBuild : async build => {
        return _normalize(await _mongo.insert(constants.TABLENAME_BUILDS, _denormalizeBuild(build)), _normalizeBuild)
    },

    getBuild : async (id, options) =>{
        return _normalize(await _mongo.getById(constants.TABLENAME_BUILDS, id, options), _normalizeBuild)
    },

    getLatestBuild: async jobId => {
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
    pageBuilds : async(jobId, index, pageSize)=>{
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
    getBuildByExternalId: async (jobId, build) =>{
        return _normalize(await _mongo.findFirst(constants.TABLENAME_BUILDS, {
            $and: [ 
                { 'jobId' :{ $eq : new ObjectID(jobId) } },
                { 'build' :{ $eq : build } }
            ]
        }), _normalizeBuild)
    },

    updateBuild : async build => {
        await _mongo.update(constants.TABLENAME_BUILDS, _denormalizeBuild(build))
    },

    removeBuild : async id => {
        // remove children
        await _mongo.remove(constants.TABLENAME_BUILDINVOLVEMENTS, { 
            buildId : new ObjectID(id) 
        })

        // remove record
        await _mongo.remove(constants.TABLENAME_BUILDS, { 
            _id : new ObjectID(id) 
        })
    },

    /**
     * Gets finished builds with no delta, but also the last build with a delta to server as delta start
     */
    getBuildsWithNoDelta: async()=>{
        let lastWithDelta = (await _mongo.aggregate(constants.TABLENAME_BUILDS, 
            {
                $match : {
                    $and: [ 
                        { 'delta' :{ $ne : null } }
                    ]            
                }
            },
            {
                $sort: { 'started': -1 } // sort latest first
            },
            {
                $limit: 1
            })).pop()

        // return all null deltas, and if exists, last with delta to server as starting history
        return _normalize(await _mongo.aggregate(constants.TABLENAME_BUILDS, 
            {
                $match : {
                    $and: [ 
                        // builds with no delta or last with delta
                        { 
                            $or : [
                                { '_id' :{ $eq : lastWithDelta ? lastWithDelta._id : null } },
                                { 'delta' :{ $eq : null } } 
                            ]
                        },

                        // finished builds only - either passed or failed
                        {
                            $or : [
                                { 'status' :{ $eq : constants.BUILDSTATUS_FAILED } },
                                { 'status' :{ $eq : constants.BUILDSTATUS_PASSED } }
                            ]
                        }
                    ]            
                }
            },

            {
                // sort earliest first
                $sort: { 'started': 1 }
            }

        ), _normalizeBuild)
    },

    /**
     * Gets the build that broke a job. Returns null if the job is not broken or has never run.
     * 
     * This works by finding the last known passing build in the job, and then takes the earliest _subsequent_ build which
     * failed.
    */
   getCurrentlyBreakingBuild : async jobId =>{
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
    getUnmappedBuildInvolvements : async()=> {
        return _normalize(await _mongo.find(constants.TABLENAME_BUILDINVOLVEMENTS, {
            $and: [ 
                { 'userId' :{ $eq : null } }
            ]
        }), _normalizeBuildInvolvement)
    },

    insertBuildInvolvement : async record => {
        return _normalize(await _mongo.insert(constants.TABLENAME_BUILDINVOLVEMENTS, _denormalizeBuildInvolvement(record)), _normalizeBuildInvolvement)
    },

    updateBuildInvolvement : async record => {
        await _mongo.update(constants.TABLENAME_BUILDINVOLVEMENTS, _denormalizeBuildInvolvement(record))
    },
    
    getBuildInvolvementByExternalUsername : async (buildId, externalUsername)=>{
        return _normalize(await _mongo.findFirst(constants.TABLENAME_BUILDINVOLVEMENTS, {
            $and: [
                { 'externalUsername' :{ $eq : externalUsername } },
                { 'buildId' :{ $eq : new ObjectID(buildId) } }
            ]
        }), _normalizeBuildInvolvement)
    },

    /**
     * Gets builds that a giver user has been mapped to
     */
    getBuildInvolvementByUserId : async userId =>{
        // build.id <> buildInvolvement.buildId where  buildInvolvement.userId === user.id 
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

    getBuildInvolementsByBuild: async (buildId)=>{
        return _normalize(await _mongo.find(constants.TABLENAME_BUILDINVOLVEMENTS, {
            $and: [
                { 'buildId' :{ $eq : new ObjectID(buildId) } }
            ]
        }), _normalizeBuildInvolvement)
    },


    /****************************************************
     * Contact log
     ****************************************************/
    insertContactLog : async contactLog => {
        return _normalize(await _mongo.insert(constants.TABLENAME_CONTACTLOGS, _denormalizeContactLog(contactLog)), _normalizeContactLog)
    }, 
    
    getContactLog : async id => {
        return _normalize(await _mongo.getById(constants.TABLENAME_CONTACTLOGS, id), _normalizeContactLog)
    },

    getContactLogByContext : async (receiverContext, type, eventContext) => {
        return _normalize(await _mongo.findFirst(constants.TABLENAME_CONTACTLOGS, {
            $and: [
                { 'receiverContext' :{ $eq : receiverContext } },
                { 'type' :{ $eq : type } },
                { 'eventContext' :{ $eq : eventContext } }
            ]
        }), _normalizeContactLog)
    }, 

    pageContactLogs : async(index, pageSize)=>{
        const items = await _mongo.find(constants.TABLENAME_CONTACTLOGS, 
            {

            },

            {
                created : -1
            }
        )
        return _arrayToNormalizedPage(items, index, pageSize, _normalizeContactLog)
    },

    updateContactLog : async contactLog => {
        await _mongo.update(constants.TABLENAME_CONTACTLOGS, _denormalizeContactLog(contactLog))
    },

    clearContactLog : async beforeDate => {
        await _mongo.remove(constants.TABLENAME_CONTACTLOGS, { 
            $and: [
                { 'created' :{ $lt : beforeDate.getTime() } }
            ]
        })
    },

    /****************************************************
     * Plugin settings
     ****************************************************/
    insertPluginSetting : async setting => {
        return _normalize(await _mongo.insert(constants.TABLENAME_PLUGINSETTINGS, _denormalizePluginSetting(setting)), _normalizePluginSetting)
    }, 

    updatePluginSetting : async setting => {
        await _mongo.update(constants.TABLENAME_PLUGINSETTINGS, _denormalizePluginSetting(setting))
    }, 

    getPluginSetting : async (plugin, name) => {
        return _normalize(await _mongo.findFirst(constants.TABLENAME_PLUGINSETTINGS, {
            $and: [
                { 'plugin' :{ $eq : plugin } },
                { 'name' :{ $eq : name } }
            ]
        }), _normalizePluginSetting)
    }, 

    getPluginSettings : async (plugin) => {
        return _normalize(await _mongo.findFirst(constants.TABLENAME_PLUGINSETTINGS, {
            $and: [
                { 'plugin' :{ $eq : plugin } }
            ]
        }), _normalizePluginSetting)
    }, 

    removePluginSettings : async plugin => {
        await _mongo.remove(constants.TABLENAME_PLUGINSETTINGS, { 
            $and: [
                { 'plugin' :{ $eq : plugin } }
            ]
        })
    },
}