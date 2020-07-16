module.exports = () =>{
    return Object.assign( {}, {
        name : null,    // STRING. Name of server, for display only
        type : null,    // STRING, constant value returned by plugin
        username: null, // STRING. username (optional) if CI server requires auth
        password: null, // STRING. Password (optional) if CI server requires auth
        isEnabled: true,
        url : null,     // STRING, public URL of server
    })
}