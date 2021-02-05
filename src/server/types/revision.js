// @ts-check

/**
 * @typedef {Object} Revision
 * @property {string} revision
 * @property {Date} date
 * @property {string} user
 * @property {string} description
 * @property {Array<import("./revisionFile").RevisionFile>} files
 */
module.exports = function(){
    return Object.assign({}, {
        revision : null,    // string. Revision number / code / id, depending on source control 
        date : null,        // DATE. date revision was created.
        user : null,        // string. user name
        description : null, // string 
        files : [ ]         // array of stings, or RevisionFile with revisions that have been fault tested
    })
}


