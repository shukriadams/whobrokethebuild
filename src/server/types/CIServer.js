/**
 * @typedef {Object} CIServer
 * @property {string} name Name of server, for display only
 * @property {string} type Constant value returned by plugin. Will be something like "Jenkins" or "Drone"
 * @property {string} username Username to authenticate
 * @property {string} password Password to authenticate
 * @property {boolean} isEnabled Not currently used, consider refactoring out
 * @property {string} url Url of server. Must start with http:// or https://
 */

module.exports = class CIServer {
    
    constructor(){
        this.name = null
        this.type = null
        this.username = null
        this.password = null
        this.isEnabled = true
        this.url = null
    }

    /**
     * Returns url with embedded credentials if necessary
     */
    async getUrl(){
        const urlHelper = require(_$+'helpers/url')
        return urlHelper.ensureEmbeddedCredentials(this.url, this.username, this.password)
    }

}
