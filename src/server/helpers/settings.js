/**
 * Do not user __log in this file! Settings is the only file in the app that is loaded before the logger is available
 */


let process = require('process'),
    fs = require('fs-extra'),
    customEnv = require('custom-env'),
    constants = require(_$+ 'types/constants'),
    yaml = require('js-yaml'),
    settings = {

        // port Express listens on
        port : 3000,

        localUrl : 'http://localhost:3000',

        // root folder this app writes state / temporary data / logs etc to. Must be writeable.
        dataFolder : './data',

        //  path for WBTB's own logs
        logPath : './data/logs',

        // set to some writeable path if you want build logs from CI servers can be dumped to local text files for reference
        // path will be created if it doesn't exist
        buildLogsDump : './data/buildLogs',

        // path plugins are written to. On docker systems, this should persist. Todo : move to data folder
        pluginsPath : './data/plugins',

        // cronmask for when daemons run. By default every minute. All daemons share the same interval, but 
        // do not block each other
        daemonInterval : '* * * * *',

        // days cookies persist
        cookiesDays: 365,

        standardPageSize: 25,
        
        authType : constants.AUTHPROVIDER_INTERNAL,

        // password for master user, auto enforced on start
        adminPassword: 'admin', 


        // #######################################################################
        // dev environment only
        // #######################################################################
        bundle: true,

        bundlemode : '', // ''|.min

        // set to 'debug' for the full spam
        logLevel : 'error', 

        // set this to true to run npm install directly in plugins-internal folder. This is for dev only, 
        // and allows you to debug and step through the code in internal plugins. Do not do on this in a 
        // docker container, your modules will be destroyed when the container resets
        bindInternalPlugins : false,

        // Number of builds back in time to import. Use this to throttle import when binding to existing jobs. global. Set per job to override
        historyLimit : null, 

        // comma-separated string. use this to prevent a daemon from running. 
        daemonBlacklist : '',

        // comma-separated string. plugin names / internal daemon files which are allowed to run daemons
        daemonWhitelist : '',

        // set to false to bypass tests on start, this will greatly speed up app start, at the expense of checks. 
        checkPluginsOnStart: true,

        runDiagnosticOnStart : false,

        // in sandbox mode, all integrations will fallback to local service that shim external services.
        sandboxMode : false,

        // disable view caching to reload all views per page view
        cacheHandlebarViews : true,

        // default plugins stub - plugin data will be attached to this
        plugins: { }

    }

// Load settings from YML file, merge with default settings
if (fs.existsSync('./settings.yml')){
    let userSettings = null

    try {
        const settingsYML = fs.readFileSync('./settings.yml', 'utf8')
        userSettings = yaml.safeLoad(settingsYML)
    } catch (e) {
        console.error('Error reading settings.yml', e)
    }    
    
    settings = Object.assign(settings, userSettings)
}


// if exists, load dev .env into ENV VARs
if (fs.existsSync('./.env')){
    customEnv.env()
    console.log('.env loaded')
}

// apply all ENV VARS over settings, this means that ENV VARs win over all other settings
for (let property in settings){

    settings[property] = process.env[property] || settings[property]

    // parse env bools
    if (settings[property] === 'true')
        settings[property] = true

    if (settings[property] === 'false')
        settings[property] = false
}

// apply type fixes etc where necessary now that all settings are loaded
// this must always be an int
settings.standardPageSize = parseInt(settings.standardPageSize.toString())

module.exports = settings
