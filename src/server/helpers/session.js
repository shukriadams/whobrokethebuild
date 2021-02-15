module.exports = {

    /**
     * 
     */    
    async getCurrentUser (req){
        const constants = require(_$+ 'types/constants'),
            pluginsManager = require(_$+'helpers/pluginsManager')

        if (req.cookies[constants.COOKIE_AUTHKEY] === undefined)
            return null
            
        const data = await pluginsManager.getExclusive('dataProvider'),
            session = await data.getSession(req.cookies[constants.COOKIE_AUTHKEY])

        if (!session)
            return null

        return await data.getUser(session.userId)
    },


    /**
     * 
     */      
    async ensureRole(req, role){
        const Exception = require(_$+'types/exception'),
            user = await this.getCurrentUser(req)

        if (!user)
            throw new Exception({ message : 'unathenticated' })

        if (!user.roles.includes(role))
            throw new Exception({ message : 'unauthorized' })
    },


    /**
     * 
     */  
    async ensureUserOrRole(req, userId, role){
        const Exception = require(_$+'types/exception'),
            user = await this.getCurrentUser(req)

        if (!user)
            throw new Exception({ message : 'unathenticated' })

        if (user.id === userId)
            return

        if (!user.roles.includes(role))
            throw new Exception({ message : 'unauthorized' })
    },


    /**
     * 
     */  
    async getCurrentSession (req){
        const constants = require(_$+ 'types/constants'),
            pluginsManager = require(_$+'helpers/pluginsManager')

        if (req.cookies[constants.COOKIE_AUTHKEY] === undefined)
            return null
            
        const data = await pluginsManager.getExclusive('dataProvider'),
            session = await data.getSession(req.cookies[constants.COOKIE_AUTHKEY])
            
        return session
    }
}