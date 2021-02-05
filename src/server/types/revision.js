/**
 * @typedef {Object} Revision
 * @property {string} revision Revision number / code / id, depending on source control 
 * @property {Date} date Date revision was created.
 * @property {string} user User name
 * @property {string} description Revision message
 * @property {Array<import("./revisionFile").RevisionFile>} files Files in revisions 
 */
module.exports = class Revision {
    constructor(){
        this.revision = null    
        this.date = null        
        this.user = null        
        this.description = null 
        this.files = [ ]        
    }
}
