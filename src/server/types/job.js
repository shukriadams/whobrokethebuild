/**
 * @typedef {Object} Job
 * @property {string} CIServerId Required. objectid of ciserver job is tied to
 * @property {string} VCServerId Required. objectid of vsc server jobs is tied to
 * @property {string} logParser id of plugin to use for parsing logs for this job
 * @property {string} name name of build, can change, normally pulled from CI system
 * @property {string} tags comma-separated tags, used to groups jobs for display purposes
 * @property {boolean} isEnabled if false, daemon should ignore processing
 * @property {boolean} isPublic True if job will be visible to anon users.
 * @property {boolean} isPassing true if build is passing.
 * @property {string} changeContext if build fails or passes, context of that build. used for messaging and status change updates
 * @property {object} contactMethods
 */
module.exports = class Job {

    constructor(){
        this.CIServerId = null
        this.VCServerId = null
        this.logParser = null
        this.name = null
        this.tags = ''
        this.isEnabled = true
        this.isPublic = false
        this.isPassing = false
        this.changeContext = null
        this.contactMethods = {}
    }
    
}