const test = require(_$t+ 'helpers/testbase'),
    crypto = require('crypto'),
    constants = require(_$+ 'types/constants'),
    assert = require('madscience-node-assert'),
    internalUserAuthenticator = require(_$ +'auth/internal');

test('credentials', function(testArgs){

    it('unhappy path : user not found', async () => {

        // monkeypatch : return no user for given name
        internalUserAuthenticator._data.findInternalUser = function(){
            return null;
        }
    
        // do 
        let authTest = await internalUserAuthenticator.areCredentialsValid('myname', 'mypassword');
    
        // test
        assert.equal(authTest.result, constants.LOGINRESULT_INVALIDCREDENTIALS);
    });
    
    
    it('happy path : login works', async () => {
    
        // monkeypatch : generate hash for test password+salt
        let sha512 = crypto.createHmac('sha512', 'mysalt');
        sha512.update('mypassword');
        let hash = sha512.digest('hex');
    
        internalUserAuthenticator._data.findInternalUser = function(){
            return {
                authMethod : {
                    type : constants.AUTHPROVIDER_INTERNAL,
                    salt : 'mysalt',
                    hash : hash
                }
            };
        }
    
        // do 
        let authTest = await internalUserAuthenticator.areCredentialsValid('myname', 'mypassword');
    
        // test
        assert.equal(authTest.result, constants.LOGINRESULT_SUCCESS);
    });
});
