const encryption = require(_$+'helpers/encryption')

module.exports = {

    /**
     * Fixed format of URL - if username and password are defined, url will be converted 
     * from http[s]://example.com to
     * http[s]://username:password@example.com
     */
    async ensureEmbeddedCredentials(baseUrl, username, password){

        // apply credentials only if they haven't already be applied
        const match = baseUrl.match(/^http[s]?:\/\/.*?[:].*?@.*?/)
        if (username && password && !match){
            const decryptedPassword = await encryption.decrypt(password)
            return baseUrl.replace('://', `://${username}:${decryptedPassword}@`) 
        }

        return baseUrl

    }
}