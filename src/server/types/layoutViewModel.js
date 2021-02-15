/**
 * @typedef {Object} LayoutViewModel
 * @property {string} bundlemode 
 * @property {Array<import("./pluginInfo").PluginLink>} pluginLinks Links to plugins
 */

module.exports = class SessionViewModel {
    constructor(){
        this.bundlemode = ''
        this.pluginLinks = []
    }
}