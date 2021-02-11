let appendCommonViewModel = require(_$+ 'helpers/appendCommonViewModel'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+'helpers/handlebars'),
    daemonManager = require(_$+'daemon/manager'),
    dataHelper = require(_$+'helpers/data'),
    pluginConf

module.exports = function(app){
    app.get('/settings/system/daemon/stop', async (req, res)=>{
        try {
            daemonManager.stopAll()
            res.redirect('/settings/system')
        } catch (ex) {
            errorHandler(res,ex)
        }
    })

    app.get('/settings/system/daemon/start', async (req, res)=>{
        try {
            daemonManager.startAll()
            res.redirect('/settings/system')
        } catch (ex) {
            errorHandler(res,ex)
        }
    })


    app.delete('/settings/system/builds', async (req, res)=>{
        try {
            await dataHelper.deleteAllBuilds()
            res.end('builds cleared1')
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
            model.daemonRunning = daemonManager.isRunning()

            await appendCommonViewModel(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res,ex)
        }
    })
}

