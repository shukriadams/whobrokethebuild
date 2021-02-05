// @ts-check

module.exports = function(){
    return Object.assign({}, {
        CIServerId: null,       // STRING. Required. objectid of ciserver job is tied to
        VCServerId: null,       // STRING. Required. objectid of vsc server jobs is tied to
        logParser: null,        // STRING. id of plugin to use for parsing logs for this job
        name: null,             // STRING name of build, can change, normally pulled from CI system
        tags: '',               // STRING comma-separated tags, used to groups jobs for display purposes
        isEnabled: true,        // BOOL. if false, daemon should ignore processing
        isPublic : false,       // BOOL. True if job will be visible to anon users.
        isPassing : false,      // BOOL. true if build is passing.
        changeContext : null,   // STRING. if build fails or passes, context of that build. used for messaging and status change updates
        contactMethods : {}
    })
}