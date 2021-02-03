let commonModelHelper = require(_$+ 'helpers/commonModels'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+'helpers/handlebars'),
    daemonManager = require(_$+'daemon/manager'),
    pluginConf

module.exports = function(app){
    app.get('/settings/system/daemon/stop', async (req, res)=>{
        try {
            daemonManager.stop()
            res.redirect('/settings/system')
        } catch (ex) {
            errorHandler(res,ex)
        }
    })

    app.get('/settings/system/daemon/start', async (req, res)=>{
        try {
            daemonManager.start()
            res.redirect('/settings/system')
        } catch (ex) {
            errorHandler(res,ex)
        }
    })

    app.get('/settings/system', async function(req, res){
        try {
            const view = await handlebars.getView('settings/system'),
                data = await pluginsManager.getExclusive('dataProvider'),
                model = {
                    pluginLinks : {}
                }

            // get plugins with UI's
            if (!pluginConf)
                pluginConf = Object.freeze(pluginsManager.getPluginConf())
                
            model.pluginLinks = Object.assign({}, pluginConf)
            for(const plugin in model.pluginLinks)
                if (!model.pluginLinks[plugin].hasUI)
                    delete model.pluginLinks[plugin]

            model.ciservers = await data.getAllCIServers()
            model.vcservers = await data.getAllVCServers()
            model.daemonRunning = daemon.isRunning()

            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res,ex)
        }
    })
}

