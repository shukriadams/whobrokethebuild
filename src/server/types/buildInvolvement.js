/**
 * @typedef {Object} BuildInvolment
 * @property {string} externalUsername some user id from external system, such as SVN. always set
 * @property {string} revision the revision id the build involvement was based on.
 * @property {import("./revision").Revision} revisionObject nullable. the revision object retrieved from source control
 * @property {string} userId objectId of User. can be null if user cannot be mapped
 * @property {string} involvement must be a constants.BUILDINVOLVEMENT_*. Always set.
 * @property {boolean} ignoreFromBreakHistory should be used only to hide people if their changes are coincidentally involved.
 * @property {string} blame must be a constants.BLAME_*. Always set.
 * @property {string} comment 
 */
module.exports = class BuildInvolment{
    constructor(){
        this.externalUsername = null
        this.revision = null
        this.revisionObject = null
        this.userId = null
        this.involvement = null
        this.ignoreFromBreakHistory = false
        this.blame = null
        this.comment = null                   
    }
}
