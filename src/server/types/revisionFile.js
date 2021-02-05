// @ts-check

/**
 * 
 * @typedef {Object} RevisionFile
 * @property {string} file
 * @property {string} change
 * @property {number} faultChance
 */
module.exports = function(){


    return Object.assign({}, {
        file : null,     // string. path of file in version control system
        change : null,   // string. change in file for this revision
        faultChance: 0   // float. Chance of (0-1) that this file change was responsible for build error, if the given revision caused a build error
    })
}


