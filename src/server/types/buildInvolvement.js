module.exports = function(){
    return Object.assign({}, {
        buildId: null,                  // string, objectId of Build. always set
        externalUsername: null,         // STRING. some user id from external system, such as SVN. always set
        revisionId : null,              // STRING. the revision id the build involvement was based on.
        userId : null,                  // STRING. objectId of User. can be null if user cannot be mapped
        involvement: null,              // STRING, must be a constants.BUILDINVOLVEMENT_*. Always set.
        ignoreFromBreakHistory: false,  // BOOL. should be used only to hide people if their changes are coincidentally involved.
        blame: null,                    // STRING. must be a constants.BLAME_*. Always set.
        comment: null                   // STRING
    })
}