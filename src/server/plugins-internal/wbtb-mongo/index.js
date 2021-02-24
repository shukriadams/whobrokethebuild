/**
 * This module hides our data layer's query logic, and thereby its type. Currently this is
 * build on mongo, but we can replace that with other providers in time.
 */
const constants = require(_$+'types/constants'),
    ObjectID = require('mongodb').ObjectID,
    settings = require(_$+'helpers/settings'),
    CIServer = require(_$+'types/CIServer'),
    VCServer = require(_$+'types/VCServer'),
    Session = require(_$+'types/session'),
    ContactLog = require(_$+'types/contactLog'),
    Job = require(_$+'types/job'),
    Build = require(_$+'types/build'),
    User = require(_$+'types/user'),
    thisType = 'wbtb-mongo',
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
    _normalizeJob = record =>{
        const job = Object.assign(new Job(), record)
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
    _normalizeBuild = record =>{
        const build = Object.assign(new Build(), record)
        build.id = build._id.toString()
        build.jobId = build.jobId.toString()
        delete build._id

        for (let buildInvolvement of build.involvements)
            if (buildInvolvement.userId)
                buildInvolvement.userId = buildInvolvement.userId.toString()

        return build
    }, 
    _denormalizeBuild = record =>{
        const build = Object.assign({}, record)
        if (build.id){
            build._id = new ObjectID(build.id)
            delete build.id
        }

        build.jobId = new ObjectID(build.jobId)
        for (let buildInvolvement of build.involvements)
            if (buildInvolvement.userId)
                buildInvolvement.userId = new Object(buildInvolvement.userId)

        return build
    },
    _normalizeVCServer = record =>{
        const vcServer = Object.assign(new VCServer(), record)
        vcServer.id = vcServer._id.toString()
        delete vcServer._id
        return vcServer
    },
    _denormalizeVCServer = record =>{
        const vcServer = Object.assign({}, record)
        if (vcServer.id){
            vcServer._id = new ObjectID(vcServer.id)
            delete vcServer.id
        }         
        return vcServer
    },     
    _normalizeCIServer = rawRecord =>{
        const ciserver = Object.assign( new CIServer(), rawRecord)
        ciserver.id = ciserver._id.toString()
        delete ciserver._id
        return ciserver
    },
    _denormalizeCIServer = record =>{
        const server = Object.assign({}, record)
        if (server.id){
            server._id = new ObjectID(server.id)
            delete server.id
        }         
        return server
    },
    _normalizeUsers = record =>{
        const user = Object.assign( new User(), record)
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
    _normalizeSession = record =>{
        const session = Object.assign( new Session(), record)
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
    _normalizeContactLog = record =>{
        const contactLog = Object.assign( new ContactLog(), record)
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
        if (!settings.plugins[thisType].connectionString){
            __log.error(`Plugin "${thisType}" requires "connectionString" with format "mongodb://USER:PASSWORD@HOST:PORT"`)
            return false
        }
        
        if (!settings.plugins[thisType].db){
            __log.error(`Plugin "${thisType}" requires "db" with name of database to use`)
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

    /**
     * Gets users with the given plugin name configured for them
     */
    async getUsersUsingPlugin(pluginName){
        const pluginSettings = {}
        pluginSettings[`pluginSettings.${pluginName}`] = { $ne : null }

        return _normalize(await _mongo.find(constants.TABLENAME_USERS, {
            $and: [ pluginSettings ]
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
     * @param {object} build 
     * Gets build after the given build
     */
    async getNextBuild(build) {
        return _normalize((await _mongo.aggregate(constants.TABLENAME_BUILDS, 
            {
                $match : {
                    $and: [ 
                        { 'jobId' :{ $eq : new ObjectID(build.jobId) } },
                        { 'started' :{ $gt : build.started } }
                    ],
                }
            },

            {
                $sort: { 'started': 1}
            },

            {
                $limit: 1
            }
        )).pop(), _normalizeBuild)
    },


    /**
     * @param {object} build 
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
     * Pages through builds. Note that this is inefficient as it pages in server memory instead of in mongo, but oh yeah,
     * mongo can't page
     */
    async pageBuilds (jobId, index, pageSize){
        let items = await _mongo.find(constants.TABLENAME_BUILDS, 
            {
                $and: [ 
                    { jobId : { $eq : new ObjectID(jobId) } }
                ]
            },
            {
                started : -1
            }
        )

        // calculate page count based on total nr of items returned
        let pages = Math.floor(items.length / pageSize)
        if (items.length % pageSize)
            pages ++

        // take page
        items = items.slice(index * pageSize, (index * pageSize) + pageSize)

        items = _normalize(items, _normalizeBuild)

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
        // remove record
        await _mongo.remove(constants.TABLENAME_BUILDS, { 
            _id : new ObjectID(id) 
        })
    },

    async removeAllBuilds(){
        // remove record
        await _mongo.remove(constants.TABLENAME_BUILDS)
    },

    /**
     * A build's log must already be fetched (ie, be not null) to qualify
     */
    async getBuildsWithUnparsedLogs(){
        return _normalize(await _mongo.find(constants.TABLENAME_BUILDS, {
            $and: [ 
                { 'logPath' :{ $eq : null } }
            ]
        }), _normalizeBuild)
    },


    async getBuildsWithUnprocessedLogs(){
        return _normalize(await _mongo.find(constants.TABLENAME_BUILDS, {
            $and: [ 
                { 'logStatus' :{ $eq : constants.BUILDLOGSTATUS_UNPROCESSED } }
            ]
        }), _normalizeBuild)
    },
    

    /**
     * Gets finished builds with no delta
     */
    async getBuildsWithNoDelta(){
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
            },
            {
                $sort: { 'started': 1 } // sort oldest first
            },

        ), _normalizeBuild)
    },


    /**
     * Gets builds that have completed (passed or failed), but which have no build involvements and therefore no revisions 
     * associated with them.
     */
    async getResolvedBuildsWithNoInvolvements(){
        return _normalize(await _mongo.aggregate(constants.TABLENAME_BUILDS, 
            {
                $match : {
                    $and: [ 
                        { 'revisions' : { $eq : [] }},
                        { 'processStatus' : { $eq : null }},
                        {
                            // finished builds only - either passed or failed
                            $or : [
                                { 'status' : { $eq : constants.BUILDSTATUS_FAILED } },
                                { 'status' : { $eq : constants.BUILDSTATUS_PASSED } }
                            ]
                        }
                    ]            
                }
            }

        ), _normalizeBuild)
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
                // sort latest first
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
    async getBuildsWithUnmappedInvolvements (){
        return _normalize(await _mongo.find(constants.TABLENAME_BUILDS, {
            $and: [ 
                { 'involvements.userId' :{ $eq : null } }
            ]
        }), _normalizeBuild)
    },


    /**
     * @param {string} userId Id of User object, in string form
     * Gets builds that a giver user has been mapped to
     */
    async pageBuildsByUser (userId, index, pageSize){
        let items = await _mongo.aggregate(constants.TABLENAME_BUILDS, 
            {
                $match: {
                    $and : [
                        { "involvements.userId" :{ $eq : new ObjectID(userId)  } }
                    ] 
                }
            },
            {
                // sort latest first
                $sort: { 'started': -1 }
            }
        )

        // calculate page count based on total nr of items returned
        let pages = Math.floor(items.length / pageSize)
        if (items.length % pageSize)
            pages ++

        // take page 
        items = items.slice(index * pageSize, (index * pageSize) + pageSize)

        // normalize
        items = _normalize(items, _normalizeBuild)

        return { items, pages}
    },


    /**
     * Gets an array of builds which have already had their logs processed and are now ready to have their revision data fetched and mapped against 
     * their build results to determine which if any of those revisions were involved in a build break
     */
    async getBuildsWithoutRevisionObjects(){
        const builds = await _mongo.aggregate(constants.TABLENAME_BUILDS, 
            {
                $match: { 
                    $and: [ 
                        { 'involvements.revisionObject' :{ $eq : null }},
                        { 'logStatus' :{ $eq : constants.BUILDLOGSTATUS_PROCESSED }},
                        { 'logPath' :{ $ne : null }}
                    ] 
                }
            }
        )

        return _normalize(builds, _normalizeBuild)
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
            builds = await this.getAllBuilds()
        
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
        
        // remove all builds which have invalid jobids
        for (let i = 0 ; i < builds.length ; i ++){
            const build = builds[builds.length - 1 - i]
            if (!jobs.find(job => job.id === build.jobId)){
                await this.removeBuild(build.id)
                builds.splice(i, 1)
                __log.info(`Cleaned out orphan build ${build.id}`)
            }
        }

    }

}