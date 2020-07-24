let
    settings = require(_$+'helpers/settings'),
    jsonfile = require('jsonfile'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+'helpers/handlebars'),
    pluginConf

module.exports = function(app){
    app.get('/settings/system', async function(req, res){
        try {
            const 
                view = await handlebars.getView('settings/system'),
                data = await pluginsManager.getExclusive('dataProvider'),
                model = {
                    pluginLinks : {}
                }

            // get plugins with UI's
            if (!pluginConf){
                if (!await fs.exists('./.plugin.conf'))
                    throw `.plugin.conf not found`

                pluginConf = Object.freeze(jsonfile.readFileSync('./.plugin.conf'))
            }
                
            model.pluginLinks = Object.assign({}, pluginConf)
            for(const plugin in model.pluginLinks)
                if (!model.pluginLinks[plugin].hasUI)
                    delete model.pluginLinks[plugin]

            model.ciservers = await data.getAllCIServers()
            model.vcservers = await data.getAllVCServers()
            
            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res,ex)
        }
    })
}

