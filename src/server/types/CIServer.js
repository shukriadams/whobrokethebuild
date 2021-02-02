module.exports = () =>{
    return Object.assign( {}, {
        name : null,    // STRING. Name of server, for display only
        type : null,    // STRING, constant value returned by plugin
        username: null, // STRING. username (optional) if CI server requires auth
        password: null, // STRING. Password (optional) if CI server requires auth
        isEnabled: true,
        url : null,     // STRING, public URL of server

        /**
         * Returns url with embedded credentials if necessary
         */
        async getUrl(){
            const urlHelper = require(_$+'helpers/url')
            return urlHelper.ensureEmbeddedCredentials(this.url, this.username, this.password)
        }
    })
}