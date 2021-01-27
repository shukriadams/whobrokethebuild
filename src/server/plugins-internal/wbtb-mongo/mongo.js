const
    settings = require(_$+'helpers/settings'),
    constants = require(_$+'types/constants'),
    MongoClient = require('mongodb').MongoClient,
    ObjectID = require('mongodb').ObjectID,

    /** 
     * Gets a mongo collection, and db instance for closing.
     */
    _getCollection = async function(collectionName){
        return new Promise(function(resolve, reject){
            try {
                MongoClient.connect(settings.mongoConnectionString, { poolSize : settings.poolSize, useUnifiedTopology: true }, function(err, client) {
                    if (err)
                        return reject(err)

                    const db = client.db(settings.mongoDBName)

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
                MongoClient.connect(settings.mongoConnectionString, { poolSize : settings.poolSize, useUnifiedTopology: true }, async function(err, client) {
                    if (err)
                        return reject(err)

                    const db = client.db(settings.mongoDBName)

                    await db.collection(constants.TABLENAME_USERS).createIndex( { 'publicId': 1, 'authMethod' : 1 }, { unique: true, name : `${constants.TABLENAME_USERS}_unique` })
                    await db.collection(constants.TABLENAME_CISERVERS).createIndex( { 'name': 1 }, { unique: true, name : `${constants.TABLENAME_CISERVERS}_unique` })
                    await db.collection(constants.TABLENAME_BUILDS).createIndex( { 'jobId': 1, 'build' : 1  }, { unique: true, name : `${constants.TABLENAME_BUILDS}_unique` })
                    await db.collection(constants.TABLENAME_JOBS).createIndex( { 'name': 1, 'CIServerId' : 1  }, { unique: true, name : `${constants.TABLENAME_JOBS}_unique` })
                    await db.collection(constants.TABLENAME_BUILDINVOLVEMENTS).createIndex( { 'buildId' : 1, 'revision' : 1 }, { unique: true, name : `${constants.TABLENAME_BUILDINVOLVEMENTS}_unique` })
                    await db.collection(constants.TABLENAME_CONTACTLOGS).createIndex( { 'receiverContext' : 1, 'type' : 1, 'eventContext' : 1 }, { unique: true, name : `${constants.TABLENAME_CONTACTLOGS}_unique` })
                    

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

            try {
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
