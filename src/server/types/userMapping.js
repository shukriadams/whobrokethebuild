// @ts-check

/**
 * @typedef {Object} UserMapping
 * @property {string} userId user identifier in external system
 * @property {string} externalName user name on vcserver
 * @property {string} VCServerId vcsserver id. OPTIONAL. If not set userid is valid for all vcservers
 */
module.exports = class UserMapping {
    constructor(){
        this.userId = null
        this.externalName = null
        this.VCServerId = null
    }
}
