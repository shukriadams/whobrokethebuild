// @ts-check
/**
 * @typedef {Object} PluginSetting
 * @property {string} plugin Required. unique if of plugin this setting belongs to
 * @property {string} name Required. name of setting
 * @property {string} value
 */

module.exports = class PluginSetting {

    constructor(){
        this.plugin = null
        this.name = null
        this.value = null
    }

}
