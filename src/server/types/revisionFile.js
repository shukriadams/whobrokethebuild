/**
 * @typedef {Object} RevisionFile
 * @property {string} file path of file in version control system
 * @property {string} change change in file for this revision
 * @property {number} faultChance Chance of (0-1) that this file change was responsible for build error, if the given revision caused a build error
 * @property {boolean} isFault if a file change is most likely to be the fault in a break, will be set to true. based on faultChance value
 */
module.exports = class RevisionFile {
    constructor(){
        this.file = null
        this.change =  null
        this.faultChance = 0
        this.isFault = false
    }
}
