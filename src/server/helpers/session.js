const constants = require(_$+ 'types/constants'),
    pluginsManager = require(_$+'helpers/pluginsManager')

module.exports = {


    /**
     * 
     */    
    getCurrentUser : async function(req){
        if (!req.cookies[constants.COOKIE_AUTHKEY])
            return null
            
        const 
            data = await pluginsManager.getByCategory('dataProvider')
            session = await data.getSession(req.cookies[constants.COOKIE_AUTHKEY])

        if (!session)
            return null

        return await data.getUser(session.userId)
    }
}