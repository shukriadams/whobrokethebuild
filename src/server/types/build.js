/**
 * @typedef {Object} Build
 * @property {string} jobId ObjectID string, parent job's id
 * @property {string} build Unique build id from CI server this build originates from
 * @property {string} incidentId ObjectID of build (including self) which caused break event, if build is in a cluster of breaks. null by default
 * @property {Array<string>} revisions VCS revision or hash code at head when build was triggered. Can be empty.
 * @property {string} triggerType constants.BUILDTRIGGER_*. STRING, NOT ALWAYS PRESENT. event that triggered build
 * @property {number} started Ticks, when build started
 * @property {number} ended Ticks, when build ended. null if build is ongoing or hanging.
 * @property {string} host Name of machine on which build was done
 * @property {string} status constants.BUILDSTATUS_*. Status of build from CI server
 * @property {string} logStatus constants.BUILDLOGSTATUS_*. Status of build log 
 * @property {string} delta constants.BUILDSTATUS_*. Status of build relative to other builds. WBTB assigns this.
 * @property {Array<object>} involvements Array of BuildInvolvement objects. 
 * @property {boolean} ignoreFromBreakHistory if true, build does not count towards break history. Use this to ignore activity that we know didn't break anything
 * @property {string} comment admin comments
 * @property {string} processStatus Status of build for internal processing. Will be set to some value if it cannot be processed further, so we don't keep reprocessing it
 * @property {Array<import("./parsedBuildLogLine").ParsedBuildLogLine>} logItems Parsed log. Null if not processed.
 * @property {string} logPath REFACTOR OUT if not null, path to log file within local log store folder
 * 
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
        this.incidentId = null
        this.revisions = []
        this.triggerType = constants.BUILDTRIGGER_OTHER
        this.started = null
        this.ended = null
        this.host = null
        this.status = constants.BUILDSTATUS_OTHER
        this.logStatus = constants.BUILDLOGSTATUS_NOT_FETCHED
        this.delta = null
        this.involvements = []
        this.ignoreFromBreakHistory = false
        this.comment = null
        this.processStatus = null
        this.logItems = null
        this.logPath = null
    }
}
