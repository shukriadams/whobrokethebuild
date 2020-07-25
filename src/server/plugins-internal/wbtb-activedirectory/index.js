/**
 * Providers active directory-based access. 
 */

let
    pluginsManager = require(_$+'helpers/pluginsManager'),
    constants = require(_$+'types/constants'),
    authMethod = 'wbtb-activedirectory',
    settings = require(_$+ 'helpers/settings')

module.exports = {

    
    /**
     * gets an array of user objects from remote active directory
     */
    getAllRemoteUsers : async () => {
        const 
            ActiveDirectory = settings.sandboxMode ? require('./activeDirectoryMock') : require('node-ad-tools').ActiveDirectory,
            data = await pluginsManager.getExclusive('dataProvider'),
            activeDirectory = new ActiveDirectory({
                url: settings.activeDirectoryUrl,
                base: settings.activeDirectoryBase
            })

        const loginAttempt = await activeDirectory.loginUser(settings.activeDirectoryUser, settings.activeDirectoryPassword)
        if(!loginAttempt.success) 
            throw loginAttempt

        // omit search string to get all users back, else CN must be equal to exact user name
        const userQuery = await activeDirectory.getAllUsers(settings.activeDirectoryUser, settings.activeDirectoryPassword, settings.activeDirectorySearch, true)
        if(!userQuery.success) 
            throw userQuery

        // mark imported users with isImport flag
        for (let user of userQuery.users){
            const queryUser = await data.getByPublicId(user.guid, authMethod)
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


    processLoginRequest : async (username, password) => {
        const ActiveDirectory = settings.sandboxMode ? require('./activeDirectoryMock') : require('node-ad-tools').ActiveDirectory,
            data = await pluginsManager.getExclusive('dataProvider'),
            activeDirectory = new ActiveDirectory({
                url: settings.activeDirectoryUrl,
                base: settings.activeDirectoryBase
            })
    
        const response = await activeDirectory.loginUser(username, password)
        let userId = null,
            result = response.success ? 
                constants.LOGINRESULT_SUCCESS : 
                constants.LOGINRESULT_INVALIDCREDENTIALS 

        if (response.success){
            //get user via AD guid, then return id
            const adUser = ActiveDirectory.createUserObj(response.entry)
            const user = await data.getByPublicId(adUser.guid, authMethod)
            if (user)
                userId = user.id
        }

        return {
            result,
            userId
        }
    },
    

    validateSettings : async ()=>{
        return new Promise((resolve, reject)=>{
            try{
                if (!settings.activeDirectoryUrl)
                    throw '"activeDirectoryUrl" value missing from settings'

                if (!settings.activeDirectoryBase)
                    throw '"activeDirectoryBase" value missing from settings'

                if (!settings.activeDirectoryUser)
                    throw '"activeDirectoryUser" value missing from settings'

                if (!settings.activeDirectoryPassword)
                    throw '"activeDirectoryPassword" value missing from settings'

                resolve(true)
    
            }catch(ex){
                reject(ex)
            }
        })
    }
}