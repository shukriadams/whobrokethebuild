let 
    pluginsManager = require(_$+'helpers/pluginsManager'),
    randomstring = require('randomstring'),
    constants = require('./../types/constants'),
    AuthMethod = require('./../types/authMethod'),
    crypto = require('crypto'),
    User = require('./../types/user'),
    _processPassword = function(user){
        if (!user.password)
            throw 'user.password is required'

        if (user.authMethod !== constants.AUTHPROVIDER_INTERNAL)
            throw 'missing or invalid authMethod'

        if (!user.authData.salt)
            user.authData.salt = randomstring.generate(24)

        let sha512 = crypto.createHmac('sha512', user.authData.salt)
        sha512.update(user.password)
        user.authData.hash = sha512.digest('hex')

        delete user.password
        return user
    }

module.exports = {
    
    async initializeAdmin (){
        // enforce master password
        let data = await pluginsManager.getExclusive('dataProvider'),
            settings = require(_$+'helpers/settings'),
            user = await data.getByPublicId(constants.ADMINUSERNAME, 'AUTHPROVIDER_INTERNAL')

        if (!user){
            user = await this.createInternal(constants.ADMINUSERNAME, settings.adminPassword)
            __log.info(`internal admin user autocreated`)
        }

        user.password = settings.adminPassword
        user.isAuthApproved = true
        
        if (!user.roles.includes(constants.ROLE_ADMIN))
            user.roles.push(constants.ROLE_ADMIN)

        await this.update(user)
    },

    async createInternal(name, password){
        const data = await pluginsManager.getExclusive('dataProvider')

        let user = new User()
        user.authData = AuthMethod(constants.AUTHPROVIDER_INTERNAL)
        user.authMethod = constants.AUTHPROVIDER_INTERNAL
        user.name = name
        user.password = password
        user.publicId = name
        user = _processPassword(user)
        await data.insertUser(user)
        return user
    },

    async update (user){
        const data = await pluginsManager.getExclusive('dataProvider')
        user = _processPassword(user)
        await data.updateUser(user)
    },
    
    async remove (user){
        const data = await pluginsManager.getExclusive('dataProvider')
        await data.removeUser(user)
    },


    /**
     * revokes user's ability to log in
     */
    async reject (user){
        const data = await pluginsManager.getExclusive('dataProvider')
        user.isAuthApproved = false
        await data.updateUser(user)
    },


    async getAll (){
        const data = await pluginsManager.getExclusive('dataProvider')
        await data.getAllUsers()
    },

    async approve (user){
        const data = await pluginsManager.getExclusive('dataProvider')
        user.isAuthApproved = true
        await data.updateUser(user)
    }
}