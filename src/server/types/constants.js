module.exports = Object.freeze({
    ADMINUSERNAME : 'admin',

    AUTHPROVIDER_AD : 'AUTHPROVIDER_AD',
    AUTHPROVIDER_INTERNAL : 'AUTHPROVIDER_INTERNAL',
    AUTHPROVIDER_NONE : 'AUTHPROVIDER_NONE', // used for autogenerated users, cannot log in.
    
    AVATARTYPE_OTHER : 'AVATARTYPE_OTHER',
    
    BUILDTRIGGER_CHANGE : 'BUILDTRIGGER_CHANGE',
    BUILDTRIGGER_MANUAL : 'BUILDTRIGGER_MANUAL',
    BUILDTRIGGER_OTHER : 'BUILDTRIGGER_OTHER',
    
    BUILDSTATUS_FAILED : 'BUILDSTATUS_FAILED',
    BUILDSTATUS_PASSED : 'BUILDSTATUS_PASSED',
    BUILDSTATUS_INPROGRESS : 'BUILDSTATUS_INPROGRESS',
    BUILDSTATUS_OTHER : 'BUILDSTATUS_OTHER',
    
    BUILDINVOLVEMENT_SOURCECHANGE : 'BUILDINVOLVEMENT_SOURCECHANGE',
    BUILDINVOLVEMENT_SUSPECTED_SOURCECHANGE : 'BUILDINVOLVEMENT_SUSPECTED_SOURCECHANGE',
    BUILDINVOLVEMENT_ASSISTING : 'BUILDINVOLVEMENT_ASSISTING',
    
    // everyone is a suspect in the eyes of the law in the event of a build break!
    BLAME_SUSPECT : 'BLAME_SUSPECT',
    // it was them what done it
    BLAME_GUILTY : 'BLAME_GUILTY',
    // present, but somehow didn't cause it
    BLAME_INNOCENT : 'BLAME_INNOCENT',

    CISERVERTYPE_JENKINS : 'CISERVERTYPE_JENKINS',
    CISERVERTYPE_OTHER : 'CISERVERTYPE_OTHER',
    
    CONTACTTYPE_EMAIL : 'CONTACTTYPE_EMAIL',
    CONTACTTYPE_SLACK : 'CONTACTTYPE_SLACK',
    
    COOKIE_AUTHKEY : 'authed',
    COOKIE_AUTHVALUE : 'true',

    ERROR_DEFAULT : 'error_default',
    // expected plugin or plugin category not installed
    ERROR_MISSINGPLUGIN : 'error_missingplugin',

    RESPONSECODES_MISSINGDATA : 'MISSING_DATA',
    RESPONSECODES_INVALIDCREDENTIALS : 'INVALID_CREDENTIALS',
    RESPONSECODES_LOGINSUCCESS : 'LOGIN_SUCCESS',

    ROLE_ADMIN: 'admin',

    LOGINRESULT_SUCCESS : 'LOGINRESULT_SUCCESS',
    LOGINRESULT_INVALIDCREDENTIALS : 'LOGINRESULT_INVALIDCREDENTIALS',
    LOGINRESULT_USERNOTMAPPED : 'LOGINRESULT_USERNOTMAPPED', // login succeeded but credentials from login not mapped to user
    LOGINRESULT_TOOMANYATTEMPTS : 'LOGINRESULT_TOOMANYATTEMPTS',
    LOGINRESULT_OTHER : 'LOGINRESULT_OTHER',

    TABLENAME_BUILDS : 'Builds',
    TABLENAME_BUILDINVOLVEMENTS : 'BuildInvolvements',
    TABLENAME_CONTACTLOGS : 'ContactLogs',
    TABLENAME_JOBS : 'Jobs',
    TABLENAME_PLUGINSETTINGS : 'PluginSettings',
    TABLENAME_USERS : 'Users',
    TABLENAME_SESSIONS : 'Sessions',
    TABLENAME_CISERVERS : 'CIServers',
    TABLENAME_VCSERVERS : 'VCServers',

    // previous build passed, this build passed
    BUILDDELTA_PASS : 'BUILDDELTA_PASS',
    // previous build broke, this build passed
    BUILDDELTA_FIX : 'BUILDDELTA_RESTORE',
    // previous build passed, this build broke
    BUILDDELTA_CAUSEBREAK : 'BUILDDELTA_CAUSEBREAK',
    // previus build broke, this build broke with same error
    BUILDDELTA_CONTINUEBREAK : 'BUILDDELTA_CONTINUEBREAK',
    // previous build broke, this build broke with different error. Experimental.
    BUILDDELTA_CHANGEBREAK : 'BUILDDELTA_CHANGEBREAK',

    VCSTYPE_GIT : 'VCSTYPE_GIT',
    VCSTYPE_PERFORCE : 'VCSTYPE_PERFORCE',
    VCSTYPE_SVN : 'VCSTYPE_SVN'
});
