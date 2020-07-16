const 
    process = require('process'),
    customEnv = require('custom-env'),
    constants = require(_$+ 'types/constants');

// defaults
let settings = {
    port : 3000,
    sandboxMode : false,
    poolSize : 10,
    bundle: true,
    runDiagnosticOnStart : false,
    enableDevPlugins: false,
    daemonInterval : '* * * * *',
    cookiesDays: 365,
    standardPageSize: 10,
    dataFolder : './_data',
    logPath : './_data/logs',
    externalPluginsFolder : './server/plugins',
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
    
    postgresHost: null,
    postgresPort: null,
    postgresDatabase: null,
    postgresUser: null,
    postgresPassword: null,
    
    forceReloadViews : 'true'
};


// apply custom .env settings
customEnv.env();

// capture settings from process.env
for (let property in settings){

    settings[property] = process.env[property] || settings[property];

    // parse env bools
    if (settings[property] === 'true')
        settings[property] = true;

    if (settings[property] === 'false')
        settings[property] = false;
}

module.exports = settings;
