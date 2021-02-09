module.exports = {
    

    _initialize(){
        const NodeCache = require('node-cache')
        return new NodeCache({ stdTTL: 100, checkperiod: 120 })
    },


    /**
     * Empties cache entirely
     * 
     * @returns {Promise}
     */
    async flush(){
        const nodeCache = this._initialize()
        if (!nodeCache)
            return

        nodeCache.flushAll()
    },


    /**
     * Gets item from cache
     *
     * @returns {Promise}
     */
    async get (key) {
        const Exception = require(_$+'types/exception'),
            nodeCache = this._initialize()

        return new Promise((resolve, reject) => {

            try {
                if (!nodeCache)
                    return resolve(null)

                nodeCache.get( key, ( err, object )=>{
                    if (err)
                        return reject(new Exception({ inner : err }))

                    if (object)
                        __log.debug(`cache hit on key "${key}"`)

                    resolve(object)
                })
            } catch(ex) {
                reject(ex)
            }

        })
    },


    /**
     * Adds item to cache
     * 
     * @returns {Promise}
     */
    async add (key, object){
        const Exception = require(_$+'types/exception'),
            nodeCache = this._initialize()

        return new Promise((resolve, reject) => {
            try {
                
                if (!nodeCache)
                    return resolve()

                nodeCache.set( key, object, (err)=>{
                    if (err)
                        return reject(new Exception({ inner : err }))

                    resolve()
                })

            } catch (ex) {
                reject(ex)
            }

        })
    },

    /**
     * Removes specific item from cach
     * 
     * @returns {Promise}
     */
    async remove(key){
        const Exception = require(_$+'types/exception'),
            nodeCache = this._initialize()

        return new Promise((resolve, reject) => {

            try {
                if (!nodeCache)
                    return resolve()

                nodeCache.del( key, (err)=>{
                    if (err)
                        return reject(new Exception({ inner : err }))

                    resolve()
                })

            } catch (ex) {
                reject (ex)
            }
        })
    }
}