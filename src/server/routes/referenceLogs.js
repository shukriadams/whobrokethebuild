const errorHandler = require(_$+'helpers/errorHandler'),
    appendCommonViewModel = require(_$+ 'helpers/appendCommonViewModel'),
    faultHelper = require(_$+ 'helpers/fault'),
    handlebars = require(_$+ 'helpers/handlebars')

module.exports = function(app){


    /**
     * 
     */
    app.get('/referenceLogs', async function(req, res){
        try {
            const referenceLogsHelper = require(_$+ 'helpers/referenceLogs'),
                view = await handlebars.getView('referenceLogs'),
                model = { }

            model.referenceLogs = await referenceLogsHelper.list()

            await appendCommonViewModel(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })


    /**
     * 
     */
    app.get('/referenceLogs/:id', async function(req, res){
        try {
            const referenceLogsHelper = require(_$+ 'helpers/referenceLogs'),
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

            model.referenceLog = await referenceLogsHelper.load(req.params.id)

            if (model.activeLogParser){
                const logParser = pluginsManager.get(model.activeLogParser)
                model.parsedLog = logParser.parse(model.referenceLog.log)
                model.parsedErrors = logParser.parseErrors(model.referenceLog.log)
            }

            // if both vcs and parser are set, we can try to calculate fault chance
            if (model.activeVCSType && model.activeLogParser){
                const vcs = pluginsManager.get(model.activeVCSType)

                for (let i = 0 ; i < model.referenceLog.revisions.length ; i ++){
                    // convert revision from string to object
                    model.referenceLog.revisions[i] = await vcs.parseRawRevision(model.referenceLog.revisions[i])

                    faultHelper.processRevision(model.referenceLog.revisions[i], model.parsedErrors)
                }
            }

            model.id = req.params.id

            await appendCommonViewModel(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

