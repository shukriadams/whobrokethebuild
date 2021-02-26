/**
 * @typedef {Object} PluginLink
 * @property {string} url 
 * @property {boolean} hasAdminUI
 * @property {boolean} hasUserUI 
 * @property {string} text Display text

 */

module.exports = class PluginLink {
    constructor(){
        this.url = null
        this.hasAdminUI = false
        this.hasUserUI = false
        this.text = null
    }
}
