let 
    pluginsManager = require(_$+'helpers/pluginsManager'),
    randomstring = require('randomstring'),
    constants = require('./../types/constants'),
    AuthMethod = require('./../types/authMethod'),
    crypto = require('crypto'),
    User = require('./../types/user'),

    _processPassword = function(user){
        if (!user.password)
            throw 'user.password is required';

        if (user.authMethod !== constants.AUTHPROVIDER_INTERNAL)
            throw 'missing or invalid authMethod';

        if (!user.authData.salt)
            user.authData.salt = randomstring.generate(24);

        let sha512 = crypto.createHmac('sha512', user.authData.salt);
        sha512.update(user.password);
        user.authData.hash = sha512.digest('hex');

        delete user.password;
        return user;
    },

    initializeAdmin =  async function(){
        // enforce master password
        const data = await pluginsManager.getByCategory('dataProvider')
        
        let user = await data.getByPublicId(constants.ADMINUSERNAME, 'AUTHPROVIDER_INTERNAL');
        if (!user)
            user = await createInternal(constants.ADMINUSERNAME, settings.adminPassword);

        user.password = settings.adminPassword
        user.isAuthApproved = true
        
        if (!user.roles.includes(constants.ROLE_ADMIN))
            user.roles.push(constants.ROLE_ADMIN)

        await update(user)
    },

    createInternal = async function(name, password){
        const data = await pluginsManager.getByCategory('dataProvider')

        let user = User()
        user.authData = AuthMethod(constants.AUTHPROVIDER_INTERNAL)
        user.authMethod = constants.AUTHPROVIDER_INTERNAL
        user.name = name
        user.password = password
        user.publicId = name
        user = _processPassword(user)
        await data.insertUser(user)
        return user
    },

    update = async function(user){
        const data = await pluginsManager.getByCategory('dataProvider')
        
        user = _processPassword(user);
        await data.updateUser(user);
    },

    approve = async function(user){
        const data = await pluginsManager.getByCategory('dataProvider')

        user.isAuthApproved = true;
        await data.updateUser(user);
    },

    reject = async function(){
        const data = await pluginsManager.getByCategory('dataProvider')

        user.isAuthApproved = false;
        await data.updateUser(user);
    },

    getAll = async function(){
        const data = await pluginsManager.getByCategory('dataProvider')

        await data.getAllUsers();
    },

    remove = async function(user){
        const data = await pluginsManager.getByCategory('dataProvider')

        await data.removeUser(user);
    };

module.exports = {
    initializeAdmin,
    createInternal,
    update,
    remove,
    reject,
    getAll,
    approve
}