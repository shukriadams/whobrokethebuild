const pluginsManager = require(_$+'helpers/pluginsManager'),
    appendCommonViewModel = require(_$+ 'helpers/appendCommonViewModel'),
    jobLogic = require(_$+'logic/job'),
    handlebars = require(_$+'helpers/handlebars'),
    sessionHelper = require(_$+'helpers/session'), 
    errorHandler = require(_$+'helpers/errorHandler')

module.exports = function(app){

    app.get('/settings/job/:id?', async function(req, res){
        try {
            
            //////////////////////////////////////////////////////////
            await sessionHelper.ensureRole(req, 'admin')
            //////////////////////////////////////////////////////////

            const view = await handlebars.getView('settings/job'),
                model = { },
                data = await pluginsManager.getExclusive('dataProvider')

            if (req.params.id)
                model.job = await data.getJob(req.params.id)

            model.vcServers = await data.getAllVCServers()
            model.ciServers = await data.getAllCIServers()
            model.logParsers = await pluginsManager.getTypeCodesOf('logParser', false)
            model.logParsers.unshift('')// add blank value

            // hey, looking for contactMethod saving? it's done via the contactMethod plugin using it, f.ex, slack
            
            // name query arg indicates this is a new job being set up
            if (req.query.name)
                model.job = {
                    name : req.query.name,
                    isPublic : true
                }

            await appendCommonViewModel(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res,ex)
        }
    })


    /**
     * Creates or updates a job, returns job id.
     */
    app.post('/settings/job', async function(req, res){
        try {
            //////////////////////////////////////////////////////////
            await sessionHelper.ensureRole(req, 'admin')
            //////////////////////////////////////////////////////////

            let jobId = req.body.id
            if (req.body.id)
                 await jobLogic.update(req.body)
            else    
                jobId = (await jobLogic.insert(req.body)).id

            res.json({
                jobId
            })

        } catch(ex){
            errorHandler(res,ex)
        }
    })
}

