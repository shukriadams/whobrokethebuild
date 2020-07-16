const 
    settings = require(_$+'helpers/settings'),
    jsonfile = require('jsonfile'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+'helpers/handlebars')

module.exports = function(app){
    app.get('/settings/system', async function(req, res){
        try {
            const 
                view = await handlebars.getView('settings/system'),
                data = await pluginsManager.getByCategory('dataProvider'),
                model = {
                    pluginLinks : {}
                }

            if (await fs.exists('./.plugin.conf'))
                model.pluginLinks = jsonfile.readFileSync('./.plugin.conf')
                
            model.ciservers = await data.getAllCIServers()
            model.vcservers = await data.getAllVCServers()
            
            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res,ex)
        }
    })
}

