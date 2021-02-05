/**
 * @typedef {Object} VCServer
 * @property {string} name Name of server, for display only
 * @property {string} vcs plugin code for VCS that this job uses
 * @property {string} username username (optional) if CI server requires auth
 * @property {string} Password Password (optional) if CI server requires auth
 * @property {string} accessToken Normally used as alternative to username/password
 * @property {boolean} isEnabled 
 * @property {string} url public URL of server
 */
module.exports = class VCServer {

    constructor(){
        this.name = null
        this.vcs = null
        this.username = null
        this.password = null
        this.accessToken = null
        this.isEnabled = true
        this.url = null
    }
    
}