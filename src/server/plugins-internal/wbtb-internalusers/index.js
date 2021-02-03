/**
 * Providers internal-based authentication. This means authentication against users created and managed
 * directly by WhoBrokeTheBuild.
 */

const 
    crypto = require('crypto'),
    constants = require(_$+'types/constants'),
    pluginsManager = require(_$+'helpers/pluginsManager')


/**
 * 
 */
async function processLoginRequest(username, password, userAgent){
    const data = await pluginsManager.getExclusive('dataProvider')

    let user = await data.getByPublicId(username, constants.AUTHPROVIDER_INTERNAL)
    if (!user)
        return { result : constants.LOGINRESULT_INVALIDCREDENTIALS }

    // ensure data integrity
    if (!user.authMethod || !user.authData.salt || !user.authData.hash)
        return { result : constants.LOGINRESULT_OTHER, message : 'invalid data' }

    let sha512 = crypto.createHmac('sha512', user.authData.salt)
    sha512.update(password)

    if (sha512.digest('hex') !== user.authData.hash)
        return { result : constants.LOGINRESULT_INVALIDCREDENTIALS }

    return { 
        result : constants.LOGINRESULT_SUCCESS, 
        userId : user.id
    }
} 


async function verify(){
    __log.info('internal auth verify not implemented yet')
    // no need to do anything,  internal test always works
} 

module.exports = {

    validateSettings: async () => {
        return true
    },

    processLoginRequest,
    verify
}