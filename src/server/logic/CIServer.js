const
    encryption = require(_$+'helpers/encryption'),
    CIServer = require(_$+'types/CIServer'),
    pluginsManager = require(_$+'helpers/pluginsManager')

function fixUrl(url){
    if (!url.toLowerCase().startsWith('http://') && !url.toLowerCase().startsWith('https://'))
        return `http://${url}`

    return url
}

module.exports = {
    async insert(properties){
        const 
            data = await pluginsManager.getByCategory('dataProvider'),
            ciserver = CIServer()

        ciserver.name = properties.name
        ciserver.type = properties.type
        ciserver.username = properties.username
        ciserver.url = fixUrl(properties.url)
        ciserver.password = properties.password
        if (ciserver.password)
            ciserver.password = await encryption.encrypt(ciserver.password)

        await data.insertCIServer(ciserver)
    },

    async delete(id){

    },

    async update(properties){
        const 
            data = await pluginsManager.getByCategory('dataProvider'),
            ciserver = await data.getCIServer(properties.id, { expected : true })
        
        ciserver.name = properties.name
        ciserver.type = properties.type
        ciserver.username = properties.username
        ciserver.url = fixUrl(properties.url)

        if (properties.password){
            if (ciserver.password !== properties.password)
                ciserver.password = await encryption.encrypt(properties.password)
        } else
            ciserver.password = null

        await data.updateCIServer(ciserver)
    }
}