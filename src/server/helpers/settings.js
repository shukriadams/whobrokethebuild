/**
 * Do not user __log in this file! Settings is the only file in the app that is loaded before the logger is available
 */


let process = require('process'),
    fs = require('fs-extra'),
    customEnv = require('custom-env'),
    constants = require(_$+ 'types/constants'),
    settings = {

        // port Express listens on
        port : 3000,

        localUrl : 'http://localhost:3000',

        // root folder this app writes state / temporary data / logs etc to. Must be writeable.
        dataFolder : './data',

        //  path for WBTB's own logs
        logPath : './data/logs',

        // path plugins are written to. On docker systems, this should persist. Todo : move to data folder
        externalPluginsFolder : './server/plugins',

        // set to some writeable path if you want build logs from CI servers can be dumped to local text files for reference
        // path will be created if it doesn't exist
        buildLogsDump : './data/buildLogs',

        // cronmask for when daemons run. By default every minute. All daemons share the same interval, but 
        // do not block each other
        daemonInterval : '* * * * *',

        // days cookies persist
        cookiesDays: 365,

        standardPageSize: 25,
        
        authType : constants.AUTHPROVIDER_INTERNAL,
        adminPassword: 'admin', // password for master user, auto enforced on start
        bundlemode : '', // ''|.min

        // #######################################################################
        // pluging-specific settings. Todo : move these to plugins!
        // #######################################################################
        mongoConnectionString : 'mongodb://root:example@127.0.0.1:27017',
        mongoDBName : 'wbtb',
        mongoPoolSize : 10,

        activeDirectoryUrl : null,
        activeDirectoryBase : null,
        activeDirectoryUser : null,
        activeDirectoryPassword : null,
        activeDirectorySearch: null,
        
        postgresHost: null,
        postgresPort: null,
        postgresDatabase: null,
        postgresUser: null,
        postgresPassword: null,
        
        slackAccessToken : null,
        // if set, all channel-targetted slack messages will be sent to this channel
        slackOverrideChannelId : null,
        // if set, all user-targetted slack messages will be sent to this user
        slackOverrideUserId : null,


        // #######################################################################
        // dev environment only
        // #######################################################################
        bundle: true,

        // set to 'debug' for the full spam
        logLevel : 'error', 

        // set this to true to run npm install directly in plugins-internal folder. This is for dev only, 
        // and allows you to debug and step through the code in internal plugins. Do not do on this in a 
        // docker container, your modules will be destroyed when the container resets
        bindInternalPlugins : false,

        // Number of builds back in time to import. Use this to throttle import when binding to existing jobs. global. Set per job to override
        historyLimit : null, 

        // use this to prevent a daemon from running. comma-separated string. 
        daemonBlacklist : null,

        // set to false to bypass tests on start, this will greatly speed up app start, at the expense of checks. 
        checkPluginsOnStart: true,

        runDiagnosticOnStart : false,

        // in sandbox mode, all integrations will fallback to local service that shim external services.
        sandboxMode : false,

        // disable view caching to reload all views per page view
        cacheHandlebarViews : true

    }


// apply custom .env settings
if (fs.existsSync('./.env')){
    customEnv.env()
    console.log('.env loaded')
}


// capture settings from process.env
for (let property in settings){

    settings[property] = process.env[property] || settings[property]

    // parse env bools
    if (settings[property] === 'true')
        settings[property] = true

    if (settings[property] === 'false')
        settings[property] = false
}

// fix things that are desperately broken
// this must always be an int
settings.standardPageSize = parseInt(settings.standardPageSize.toString())

module.exports = settings
