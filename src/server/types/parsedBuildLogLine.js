/**
 * @typedef {Object} ParsedBuildLogLine
 * @property {string} text
 * @property {string} type error|warning|text
 */
module.exports = class PluginLink {
    constructor(){
        this.text = null
        this.type = null
    }
}
