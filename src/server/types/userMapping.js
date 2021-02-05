// @ts-check

module.exports = () =>{
    return Object.assign( {}, {
        userId : null,          // STRING. user identifier in external system
        externalName : null,    // STRING. user name on vcserver
        VCServerId : null,      // STRING. vcsserver id. OPTIONAL. If not set userid is valid for all vcservers
    })
}