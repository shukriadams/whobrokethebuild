/**
 * Providers active directory-based access. 
 */

const pluginsManager = require(_$+'helpers/pluginsManager'),
    constants = require(_$+'types/constants'),
    thisType = 'wbtb-activedirectory',
    settings = require(_$+ 'helpers/settings')

module.exports = {
    
    /**
     * gets an array of user objects from remote active directory
     */
    async getAllRemoteUsers(){
        const ActiveDirectory = settings.sandboxMode ? require('./activeDirectoryMock') : require('node-ad-tools').ActiveDirectory,
            data = await pluginsManager.getExclusive('dataProvider'),
            activeDirectory = new ActiveDirectory({
                url: settings.plugins[thisType].url,
                base: settings.plugins[thisType].base
            }),
            loginAttempt = await activeDirectory.loginUser(settings.plugins[thisType].user, settings.plugins[thisType].password)

        if(!loginAttempt.success) 
            throw loginAttempt

        // omit search string to get all users back, else CN must be equal to exact user name
        const userQuery = await activeDirectory.getAllUsers(settings.plugins[thisType].user, settings.plugins[thisType].password, settings.plugins[thisType].search || '', true)
        if(!userQuery.success) 
            throw userQuery

        // mark imported users with isImport flag
        for (let user of userQuery.users){
            const queryUser = await data.getByPublicId(user.guid, thisType)
            user.isImport = !!queryUser
        }

        // sort by name
        userQuery.users = userQuery.users.sort((a, b) => {
            return a.name > b.name ? 1 :
                b.name > a.name ? -1 :
                0
        })

        return userQuery.users
    },


    async processLoginRequest(username, password){
        let ActiveDirectory = settings.sandboxMode ? require('./activeDirectoryMock') : require('node-ad-tools').ActiveDirectory,
            data = await pluginsManager.getExclusive('dataProvider'),
            activeDirectory = new ActiveDirectory({
                url: settings.plugins[thisType].url,
                base: settings.plugins[thisType].base
            }),
            userId = null,
            response = await activeDirectory.loginUser(username, password),
            result = response.success ? 
                constants.LOGINRESULT_SUCCESS : 
                constants.LOGINRESULT_INVALIDCREDENTIALS 

        if (response.success){
            const user = await data.getByPublicId(username, thisType)

            if (user)
                userId = user.id
            else 
                result = constants.LOGINRESULT_USERNOTMAPPED
        }

        return {
            result,
            userId
        }
    },
    

    async validateSettings(){
        if (!settings.plugins[thisType].url){
            __log.error(`Plugin "${thisType}" requires setting "url" with format "ldap://HOST:PORT"`)
            return false
        }

        if (!settings.plugins[thisType].base){
            __log.error(`Plugin "${thisType}" requires setting "base" with format such as "dc=YOURDOMAIN,dc=local"`)
            return false
        }

        if (!settings.plugins[thisType].user){
            __log.error(`Plugin "${thisType}" requires setting "user" with read permission on domain. Username is normally formatted "user@yourdomain"`)
            return false
        }

        if (!settings.plugins[thisType].password){
            __log.error(`Plugin "${thisType}" requires setting "password" for AD user to read from domain`)
            return false
        }

        return true
    }
}