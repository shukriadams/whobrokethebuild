const constants = require(_$+ 'types/constants'),
    AUTHPROVIDER_AD = {
        username: null,                                     // string. ActiveDirectory username.
        type : constants.AUTHPROVIDER_AD
    },
    AUTHPROVIDER_GITHUB = {
        githubId: null,                                         // string
        type : constants.AUTHPAUTHPROVIDER_GITHUBROVIDER_AD //
    },
    AUTHPROVIDER_INTERNAL = {
        hash: null,                                         // STRING
        salt: null,                                         // STRING
        type : constants.AUTHPROVIDER_INTERNAL              // STRING
    },
    AUTHPROVIDER_NONE = {
        type : constants.AUTHPROVIDER_NONE
    }

module.exports = function(type){
    switch(type){
        case constants.AUTHPROVIDER_AD:
        {
            return Object.assign({}, AUTHPROVIDER_AD);
        }
        case constants.AUTHPROVIDER_GITHUB:
        {
            return Object.assign({}, AUTHPROVIDER_GITHUB);
        }
        case constants.AUTHPROVIDER_INTERNAL:
        {
            return Object.assign({}, AUTHPROVIDER_INTERNAL);
        }
        case constants.AUTHPROVIDER_NONE:
        {
            return Object.assign({}, AUTHPROVIDER_NONE);
        }
        default:
        {
            throw `${type} is not a supported auth method type.`
        }
    }
}