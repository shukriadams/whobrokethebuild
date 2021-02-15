/**
 * @typedef {Object} ParsedBuildLog
 * @property {string} error 
 * @property {Array<ParsedBuildLogLine>} lines
 */
const { default: ParsedBuildLogLine } = require(_$+'types/parsedBuildLogLine')

module.exports = class PluginLink {
    constructor(){
        this.error = null
        this.lines = []
    }
}
