const pluginsManager = require(_$+'helpers/pluginsManager'),
    Session = require(_$+ 'types/session')

module.exports = {

    async insert(userId, agent, ip){
        const data = await pluginsManager.getExclusive('dataProvider'),
            session = new Session()

        session.userId = userId
        session.userAgent = agent
        session.ip = ip
        session.created = new Date().getTime()

        return await data.insertSession(session)
    },

    async delele(id){
        const data = await pluginsManager.getExclusive('dataProvider')
        await data.removeSession(id)
    },

    async getById(id){
        const data = await pluginsManager.getExclusive('dataProvider')
        return await data.getSessionById(id)
    }
}