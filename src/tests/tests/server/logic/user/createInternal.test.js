const test = require(_$t+ 'helpers/testbase'),
    constants = require(_$+ 'types/constants'),
    userLogic = require(_$+ 'logic/users'),
    assert = require('madscience-node-assert'),
    internalUserAuthenticator = require(_$+ 'auth/internal');

test('credentials', function(testArgs){
    
    it('happy path : creates an internal user', async () => {

        // monkeypatch 
        let user;
        userLogic._data.insertUser = async function(u){
            user = u;
        };
    
        // do
        await userLogic.createInternal('myname', 'mypassword');
    
        // test : confirm data structure
        assert.equal(user.publicId, 'myname');
        assert.equal(user.authMethod, constants.AUTHPROVIDER_INTERNAL);
    
        // monkeypatch : salt + hash on new user by running these through authentication system
        internalUserAuthenticator._data.findInternalUser = function(){
            return user;
        }
    
        // do 
        let authTest = await internalUserAuthenticator.areCredentialsValid('myname', 'mypassword');
    
        // test
        assert.equal(authTest.result, constants.LOGINRESULT_SUCCESS);
    });

});