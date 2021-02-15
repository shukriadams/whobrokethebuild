/**
 * @typedef {Object} Session
 * @property {string} userId ObjectID.id of user that owns this session
 * @property {string} userAgent change in file for this revision
 * @property {string} ip Chance of (0-1) that this file change was responsible for build error, if the given revision caused a build error
 * @property {number} created long, datetime ticks session was created
 */
module.exports = class Session{

    constructor(){
        this.userId = null
        this.userAgent = null
        this.ip = null
        this.created = null
    }
    
}
