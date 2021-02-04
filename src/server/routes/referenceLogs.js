const errorHandler = require(_$+'helpers/errorHandler'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    stringSimilarity = require('string-similarity'),
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

            await commonModelHelper(model, req)
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
                model.parsedErrors = logParser.parseErrors(model.referenceLog.log)
            }

            if (model.activeVCSType){
                const vcs = pluginsManager.get(model.activeVCSType)

                if (model.referenceLog)
                    for (let i = 0 ; i < model.referenceLog.revisions.length ; i ++){
                        model.referenceLog.revisions[i] = await vcs.parseRawRevision(model.referenceLog.revisions[i])

                        for (let j = 0 ; j < model.referenceLog.revisions[i].files.length ; j ++)
                            model.referenceLog.revisions[i].files[j].faultChance = Math.floor(stringSimilarity.compareTwoStrings(
                                model.referenceLog.revisions[i].files[j].file,
                                model.referenceLog.log
                            ) * 100)
                        

                    }
            }

            model.id = req.params.id

            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

