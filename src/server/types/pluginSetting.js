module.exports = function(){
    return Object.assign({}, {
        plugin: null,   // STRING. Required. unique if of plugin this setting belongs to
        name: null,     // STRING. Required. name of setting
        value: null       // STRING. Value.
    })
}