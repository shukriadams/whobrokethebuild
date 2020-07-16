/**
 * Providers internal-based authentication. This means authentication against users created and managed
 * directly by WhoBrokeTheBuild.
 */

const 
    crypto = require('crypto'),
    constants = require(_$+'types/constants'),
    Session = require(_$+'types/session'),
    pluginsManager = require(_$+'helpers/pluginsManager')


/**
 * 
 */
async function processLoginRequest(username, password, userAgent){
    const data = await pluginsManager.getByCategory('dataProvider')

    let user = await data.getByPublicId(username, constants.AUTHPROVIDER_INTERNAL)
    if (!user)
        return { result : constants.LOGINRESULT_INVALIDCREDENTIALS }

    // ensure data integrity
    if (!user.authMethod || !user.authData.salt || !user.authData.hash)
        return { result : constants.LOGINRESULT_OTHER, message : 'invalid data' };

    let sha512 = crypto.createHmac('sha512', user.authData.salt);
    sha512.update(password);

    if (sha512.digest('hex') !== user.authData.hash)
        return { result : constants.LOGINRESULT_INVALIDCREDENTIALS };        

    // generate sessionkey
    let session = Session();
    session.userId = user.id;
    session.created = new Date().getTime();
    session.userAgent = userAgent;

    session = await data.insertSession(session);
    let result = { 
        result : constants.LOGINRESULT_SUCCESS, 
        sessionKey : session.id
    };

    return result;


} 

async function verify(){
    console.log('internal auth verify not implemented yet');
    // no need to do anything,  internal test always works
} 

module.exports = {
    getTypeCode : ()=>{
        return 'internal'
    },

    // required by plugin interface
    getDescription(){
        return {
            id : 'internal',
            name : null
        }
    },
    

    validateSettings: async () => {
        return true
    },

    processLoginRequest,
    verify
}