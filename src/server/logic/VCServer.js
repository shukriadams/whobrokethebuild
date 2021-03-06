const encryption = require(_$+'helpers/encryption'),
    VCServer = require(_$+'types/VCServer'),
    pluginsManager = require(_$+'helpers/pluginsManager')

module.exports = {
    async insert(properties){
        const data = await pluginsManager.getExclusive('dataProvider'),
            vcserver = new VCServer()

        vcserver.name = properties.name
        vcserver.vcs = properties.vcs
        vcserver.username = properties.username
        vcserver.url = properties.url
        vcserver.password = properties.password
        if (vcserver.password)
            vcserver.password = await encryption.encrypt(vcserver.password)

        vcserver.accessToken = properties.accessToken
        if (vcserver.accessToken)
            vcserver.accessToken = await encryption.encrypt(vcserver.accessToken)

        await data.insertVCServer(vcserver)
    },

    async getById (id) {
        const data = await pluginsManager.getExclusive('dataProvider')
        return await data.getVCServer(id)
    },

    async delete(id){
        const data = await pluginsManager.getExclusive('dataProvider')
        await data.removeVCServer(id)
    },

    async update(properties){
        const data = await pluginsManager.getExclusive('dataProvider'),
            vcserver = await data.getVCServer(properties.id, { expected : true })
        
        vcserver.name = properties.name
        vcserver.vcs = properties.vcs
        vcserver.username = properties.username
        vcserver.url = properties.url

        if (properties.password){
            if (vcserver.password !== properties.password)
                vcserver.password = await encryption.encrypt(properties.password)
        } else
            vcserver.password = null

        await data.updateVCServer(vcserver)
    }
}