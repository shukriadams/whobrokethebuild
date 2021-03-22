const settings = require(_$+'helpers/settings'),
    constants = require(_$+'types/constants'),
    MongoClient = require('mongodb').MongoClient,
    thisType = 'wbtb-mongo',
    poolSize = 10,
    ObjectID = require('mongodb').ObjectID,
    
    /** 
     * Gets a mongo collection, and db instance for closing.
     */
    _getCollection = async function(collectionName){
        return new Promise(function(resolve, reject){
            try {
                MongoClient.connect(settings.plugins[thisType].connectionString, { poolSize : settings.plugins[thisType].poolSize || poolSize, useUnifiedTopology: true }, function(err, client) {
                    if (err)
                        return reject(err)

                    const db = client.db(settings.plugins[thisType].db)

                    resolve({ 
                        close : ()=>{
                            client.close()
                        }, 
                        collection : db.collection(collectionName)
                    })
                })
            } catch(ex){
                reject(ex)
            }
        })
    },


    /**
     * Use this to set up database structures, import default 
     */
    initialize = async function(){
        return new Promise(async function(resolve, reject){
            try {
                MongoClient.connect(settings.plugins[thisType].connectionString, { poolSize : settings.plugins[thisType].poolSize || poolSize, useUnifiedTopology: true }, async function(err, client) {
                    if (err)
                        return reject(err)

                    const db = client.db(settings.plugins[thisType].db)

                    // unique constraints
                    await db.collection(constants.TABLENAME_BUILDS).createIndex( { 'jobId': 1, 'build' : 1  }, { unique: true, name : `${constants.TABLENAME_BUILDS}_unique` })
                    await db.collection(constants.TABLENAME_CISERVERS).createIndex( { 'name': 1 }, { unique: true, name : `${constants.TABLENAME_CISERVERS}_unique` })
                    await db.collection(constants.TABLENAME_JOBS).createIndex( { 'name': 1, 'CIServerId' : 1  }, { unique: true, name : `${constants.TABLENAME_JOBS}_unique` })
                    await db.collection(constants.TABLENAME_PLUGINSETTINGS).createIndex( { 'plugin' : 1, 'name' : 1 }, { unique: true, name : `${constants.TABLENAME_PLUGINSETTINGS}_unique` })
                    await db.collection(constants.TABLENAME_USERS).createIndex( { 'publicId': 1, 'authMethod' : 1 }, { unique: true, name : `${constants.TABLENAME_USERS}_unique` })
                    await db.collection(constants.TABLENAME_VCSERVERS).createIndex( { 'name': 1 }, { unique: true, name : `${constants.TABLENAME_VCSERVERS}_unique` })

                    // lookup-optimized indexes
                    await db.collection(constants.TABLENAME_BUILDS).createIndex( { jobId: 1, build : 1, status: 1, isLogParsed: 1, started: 1, delta : 1  }, { unique: false, name : `${constants.TABLENAME_BUILDS}_performance` })
                    await db.collection(constants.TABLENAME_CONTACTLOGS).createIndex( { 'receiverContext' : 1, 'type' : 1, 'eventContext' : 1 }, { unique: true, name : `${constants.TABLENAME_CONTACTLOGS}_performance` })
                    await db.collection(constants.TABLENAME_USERS).createIndex( { authMethod: 1, publicId: 1, 'userMappings.name' : 1, 'userMappings.VCServerId' : 1 }, { unique: false, name : `${constants.TABLENAME_USERS}_performance` })

                    client.close()
                    resolve()
                })
            } catch(ex){
                reject(ex)
            }
        })
    },

    
    /**
     * Args :
     * table, and then any number of arguments used by mongo's native aggregate method, example "aggregate, where, sort, limit".
     */
    aggregate = async function(table, aggregator){
        // advanced mongo queries are a sequence of objects, ex : aggregator, where, sort, limit, etc. Convert to array so we can do this dynamically
        const args = Array.from(arguments).slice(1)
        return new Promise(async function(resolve, reject){
            try {
                const mongo = await _getCollection(table)
              
                mongo.collection.aggregate(args, function(err, records){
                    if (err)
                        return reject(err)

                    records.toArray(function(err, records){
                        if (err)
                            return reject(err)

                        if (!records)
                            return resolve([])

                        mongo.close()
                        resolve(records)
                    })
    
                })
               
            } catch(ex){
                reject(ex)
            }
        })
    },  


    /**
     * 
     */ 
    distinct = async function(collectionName, field, query){
        return new Promise(async function(resolve, reject){
            try {
                const mongo = await _getCollection(collectionName)
                mongo.collection.distinct(field, query, function(err, records){
                    if (err)
                        return reject(err)
                    
                    mongo.close()
                    resolve(records)
                })

            } catch(ex){
                reject(ex)
            }
        })
    },


    /**
     * 
     */ 
    getById = async function(collectionName, id, options = {}){
        return new Promise(async (resolve, reject)=>{
            try {
                // if an id is corrupt/ invalid we don't want objectId to throw a parse error
                // and derail entire call - an invalid id should be treated as "not found"
                // which is a null return
                try {
                    id = new ObjectID(id)
                }catch(ex){
                    if (options.expected)
                        return reject(`Expected record id ${id} from table ${collectionName} not found`)

                    resolve(null)
                }

                const mongo = await _getCollection(collectionName)
                mongo.collection.findOne({ _id : id },(err, record)=>{
                    if (err)
                        return reject(err)
                    
                    if (options.expected && !record)
                        return reject(`Expected record id ${id} from table ${collectionName} not found`)

                    mongo.close()
                    resolve(record)
                })

            } catch(ex){
                reject(ex)
            }
        })
    },

    
    /**
     * 
     */ 
    find = async function(collectionName, query = {}, sort = {}, limit = 0){
        return new Promise(async function(resolve, reject){
            try {
                const mongo = await _getCollection(collectionName)
                
                mongo.collection.find(query).sort(sort).limit(limit).toArray(function(err, records){
                    if (err)
                        return reject(err)
                    
                    mongo.close()
                    resolve(records)
                })

            } catch(ex){
                reject(ex)
            }
        })
    },    


    /**
     * 
     */ 
    findOne = async function(collectionName, query){
        return new Promise(async function(resolve, reject){
            try {
                const mongo = await _getCollection(collectionName)
                mongo.collection.find(query).toArray(function(err, records){
                    if (err)
                        return reject(err)
                    
                    mongo.close()
                    if (records.length > 1)
                        return reject(`One or zero records expected, ${records.length} found in collection ${collectionName}`)

                    if (records.length)
                        return resolve(records[0])
                    
                    resolve(null)
                })
            } catch(ex){
                reject(ex)
            }
        })
    },


    findFirst = async function(collectionName, query){
        return new Promise(async function(resolve, reject){
            try {
                const mongo = await _getCollection(collectionName)
                mongo.collection.find(query).toArray(function(err, records){
                    if (err)
                        return reject(err)
                    
                    mongo.close()
                    resolve(records.length ? records[0] : null)
                })
            } catch(ex){
                reject(ex)
            }
        })
    },


    /**
     * 
     */ 
    remove = async function remove(collectionName, query){
        return new Promise(async function(resolve, reject){
            try {
                const mongo = await _getCollection(collectionName)
                mongo.collection.deleteMany(query, function(err){
                    if (err)
                        return reject(err)
                    
                    mongo.close()
                    resolve()
                })
            } catch(ex){
                reject(ex)
            }
        })
    },
   

    /**
     * Id must be a valid mongo ObjectId.
     */ 
    update = async function update(collectionName, record){
        return new Promise(async function(resolve, reject){
            try {
                const mongo = await _getCollection(collectionName)
                mongo.collection.updateOne({ _id : record._id }, { $set: record }, {}, function(err){
                    if (err)
                        return reject({err, record})
                    
                    mongo.close()
                    resolve()
                })
            } catch(ex){
                reject({ex, record})
            }
        })
    },

    
    /**
     * Inserts record; returns the record inserted.
     */ 
    insert = async function(collectionName, record){
        return new Promise(async function(resolve, reject){
            try {
                const mongo = await _getCollection(collectionName)

                mongo.collection.insertOne(record, function(err, result){
                    if (err)
                        return reject(err)
                        
                    mongo.close()

                    resolve(result.ops[0])
                })
            } catch(ex){
                reject(ex)
            }
        })
    }

module.exports = {
    findOne,
    findFirst,
    find,
    distinct,
    aggregate,
    initialize,
    insert,
    update,
    remove,
    getById
}
