const errorHandler = require(_$+'helpers/errorHandler'),
    viewModelHelper = require(_$+'helpers/viewModel'),
    faultHelper = require(_$+ 'helpers/fault'),
    handlebars = require(_$+ 'helpers/handlebars')

module.exports = function(app){


    /**
     * loads a page of reference log names
     */
    app.get('/referenceLogs', async function(req, res){
        try {
            const referenceLogsHelper = require(_$+ 'helpers/referenceLogs'),
                page = parseInt(req.query.page || 1) - 1, // pages are publicly 1-rooted, 0-rooted internally
                view = await handlebars.getView('referenceLogs'),
                model = { }

            model.referenceLogs = await referenceLogsHelper.page(page)
            model.referenceLogs.items = model.referenceLogs.items.map(r => encodeURIComponent(r))
            model.baseUrl = `/referenceLogs`
            await viewModelHelper.layout(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })


    /**
     * get a specific reference log
     */
    app.get('/referenceLogs/:path', async function(req, res){
        try {
            const logHelper = require(_$+'helpers/log'),
                pluginsManager = require(_$+'helpers/pluginsManager'),
                view = await handlebars.getView('referenceLog'),
                model = { }

            model.logParsers = await pluginsManager.getTypeCodesOf('logParser', false)
            model.VCSTypes = await pluginsManager.getTypeCodesOf('vcs')

            // set active log parser to the user-selected one. If user hasn't selected one, 
            // take the first available, else leave it null
            model.activeLogParser = req.query.logParser ? req.query.logParser 
                : model.logParsers.length ? model.logParsers[0] :
                null

            model.activeVCSType = req.query.vcs ? req.query.vcs
                : model.VCSTypes.length ? model.VCSTypes[0] :
                null

            if (model.activeLogParser){
                model.parsedLog = await logHelper.parseLog(decodeURIComponent(req.params.path), model.activeLogParser)
            }

            model.id = req.params.id

            await viewModelHelper.layout(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

