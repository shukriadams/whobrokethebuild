/**
 * @typedef {Object} PluginLink
 * @property {string} url 
 * @property {boolean} hasUI
 * @property {string} text Display text

 */

module.exports = class PluginLink {
    constructor(){
        this.url = null
        this.hasUI = false
        this.text = null
    }
}
