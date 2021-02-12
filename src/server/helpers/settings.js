/**
 * Do not user __log in this file! Settings is the only file in the app that is loaded before the logger is available
 */


let process = require('process'),
    fs = require('fs-extra'),
    customEnv = require('custom-env'),
    constants = require(_$+ 'types/constants'),
    settings = {
        port : 3000,
        sandboxMode : false,
        poolSize : 10,
        bundle: true,
        runDiagnosticOnStart : false,
        daemonInterval : '* * * * *',
        cookiesDays: 365,
        standardPageSize: 25,
        dataFolder : './data',
        logPath : './data/logs',
        // set to some writeable path if you want build logs from CI servers can be dumped to local text files for reference
        // path will be created if it doesn't exist
        buildLogsDump : './data/buildLogs',

        // Number of builds back in time to import. Use this to throttle import when binding to existing jobs. global. Set per job to override
        historyLimit : null, 
        externalPluginsFolder : './server/plugins',
        logLevel : 'error', // set to 'info' for full spam
        forceReloadViews : 'true',
        // set this to true to run npm install directly in plugins-internal folder. This is for dev only, 
        // and allows you to debug and step through the code in internal plugins. Do not do on this in a 
        // docker container, your modules will be destroyed when the container resets
        bindInternalPlugins : false,
        authType : constants.AUTHPROVIDER_INTERNAL,
        adminPassword: 'admin', // password for master user, auto enforced on start
        bundlemode : '', // ''|.min
        localUrl : 'http://localhost:3000',

        // PLUGIN SETTINGS : TODO - MOVE THESE TO PLUGIN

        mongoConnectionString : 'mongodb://root:example@127.0.0.1:27017',
        mongoDBName : 'wbtb',
        
        activeDirectoryUrl : null,
        activeDirectoryBase : null,
        activeDirectoryUser : null,
        activeDirectoryPassword : null,
        activeDirectorySearch: null,
        
        faultChanceThreshold: 50,
        postgresHost: null,
        postgresPort: null,
        postgresDatabase: null,
        postgresUser: null,
        postgresPassword: null,
        
        slackAccessToken : null,
        // if set, all channel-targetted slack messages will be sent to this channel
        slackOverrideChannelId : null,
        // if set, all user-targetted slack messages will be sent to this user
        slackOverrideUserId : null
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

module.exports = settings
