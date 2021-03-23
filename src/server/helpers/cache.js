let _nodeCache

module.exports = {
    

    _initialize(){
        if (!_nodeCache){
            const NodeCache = require('node-cache')
            _nodeCache = new NodeCache({ stdTTL: 100, checkperiod: 120 })
        }
    },


    /**
     * Empties cache entirely
     * 
     * @returns {Promise}
     */
    async flush(){
        this._initialize()
        _nodeCache.flushAll()
    },


    /**
     * Gets item from cache
     *
     * @returns {Promise}
     */
    async get (key) {
        const Exception = require(_$+'types/exception')
        this._initialize()

        return new Promise((resolve, reject) => {

            try {
                _nodeCache.get( key, ( err, object )=>{
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
        const Exception = require(_$+'types/exception')
        this._initialize()

        return new Promise((resolve, reject) => {
            try {
                _nodeCache.set( key, object, (err)=>{
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
        const Exception = require(_$+'types/exception')
        this._initialize()

        return new Promise((resolve, reject) => {
            try {
                _nodeCache.del( key, (err)=>{
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