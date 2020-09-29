const constants = require(_$+ 'types/constants')

/**
 * Builds don't always have revisions - on jenkins, a manually triggered build never has a build, and to figure out what code is being built we have to assume 
 * that the last preceeding build with a revision nr is the revision being built
 */    

module.exports = function(){
    return Object.assign({}, {
        jobId : null,                               // objectID, required, parent job's id
        build : null,                               // STRING. external build id
        revisions : [],                             // ARRAY (STRING), VCS revision or hash code at head when build was triggered. Can be empty.
        triggerType : constants.BUILDTRIGGER_OTHER, // STRING, NOT ALWAYS PRESENT. event that triggered build
        started : null,                             // NUMBER. Ticks, when build started
        ended : null,                               // NUMBER. Ticks, when build ended. null if build is ongoing or hanging.
        host : null,                                // STRING. Host machine on which build was done
        status : constants.BUILDSTATUS_OTHER,       // STRING. Status of build from CI server
        delta : null,                               // STRING. Status of build relative to other builds. WBTB assigns this.
        ignoreFromBreakHistory : false,             // BOOL. if true, build does not count towards break history. Use this to ignore activity that we know didn't break anything
        comment: null,                              // STRING. admin comments
        log : null                                  // STRING. Full buildlog from server. Normally compressed.
    })
}