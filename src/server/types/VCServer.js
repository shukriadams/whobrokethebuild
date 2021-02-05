// @ts-check

module.exports = () =>{
    return Object.assign( {}, {
        name : null,        // STRING. Name of server, for display only
        vcs: null,          // vcs plugin code for VCS that this job uses
        username: null,     // STRING. username (optional) if CI server requires auth
        password: null,     // STRING. Password (optional) if CI server requires auth
        accessToken : null, // STRING. Normally used as alternative to username/password
        isEnabled: true,
        url : null,         // STRING, public URL of server
    })
}