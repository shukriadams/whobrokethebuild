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
        systemDaemonInterval : '* * * * *',
        cookiesDays: 365,
        standardPageSize: 10,
        dataFolder : './_data',
        logPath : './_data/logs',
        externalPluginsFolder : './server/plugins',
        
        // if set, all channel-targetted slack messages will be sent to this channel
        slackOverrideChannelId : null,

        // if set, all user-targetted slack messages will be sent to this user
        slackOverrideUserId : null,

        // set this to true to run npm install directly in plugins-internal folder. This is for dev only, 
        // and allows you to debug and step through the code in internal plugins. Do not do on this in a 
        // docker container, your modules will be destroyed when the container resets
        bindInternalPlugins : false,

        authType : constants.AUTHPROVIDER_INTERNAL,
        adminPassword: 'admin', // password for master user, auto enforced on start
        bundlemode : '', // ''|.min
        mongoConnectionString : 'mongodb://root:example@127.0.0.1:27017',
        mongoDBName : 'wbtb',
        localUrl : 'http://localhost:3000',

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
        
        forceReloadViews : 'true'
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
