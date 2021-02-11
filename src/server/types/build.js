/**
 * @typedef {Object} Build
 * @property {string} jobId ObjectID string, parent job's id
 * @property {string} build Unique build id from CI server this build originates from
 * @property {Array<string>} revisions VCS revision or hash code at head when build was triggered. Can be empty.
 * @property {string} triggerType constants.BUILDTRIGGER_*. STRING, NOT ALWAYS PRESENT. event that triggered build
 * @property {number} started Ticks, when build started
 * @property {number} ended Ticks, when build ended. null if build is ongoing or hanging.
 * @property {string} host Name of machine on which build was done
 * @property {string} status constants.BUILDSTATUS_*. Status of build from CI server
 * @property {string} delta constants.BUILDSTATUS_*. Status of build relative to other builds. WBTB assigns this.
 * @property {boolean} ignoreFromBreakHistory if true, build does not count towards break history. Use this to ignore activity that we know didn't break anything
 * @property {string} comment admin comments
 * @property {string} log Full buildlog from server. 
 */

const constants = require(_$+ 'types/constants')

/**
 * Builds don't always have revisions - on jenkins, a manually triggered build never has a build, and to figure out what code is being built we have to assume 
 * that the last preceeding build with a revision nr is the revision being built
 */    
module.exports = class Build{
    constructor(){
        this.jobId = null
        this.build = null
        this.revisions = []
        this.triggerType = constants.BUILDTRIGGER_OTHER
        this.started = null
        this.ended = null
        this.host = null
        this.status = constants.BUILDSTATUS_OTHER
        this.delta = null
        this.ignoreFromBreakHistory = false
        this.comment = null
        this.log = null
    }
}
