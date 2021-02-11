module.exports = {
    async deleteAllBuilds(){
        const pluginsManager = require(_$+'helpers/pluginsManager'),
            data = await pluginsManager.getExclusive('dataProvider')

        await data.removeAllBuilds()
    }
}