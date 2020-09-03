const 
    settings = require(_$+ 'helpers/settings'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    buildLogic = require(_$+'logic/builds'),
    jobsLogic = require(_$+'logic/job'),
    handlebars = require(_$+ 'helpers/handlebars')

module.exports = function(app){
    
    app.delete('/build/:id', async function(req, res){
        try {
            await buildLogic.remove(req.params.id)
            res.json({})
        }catch(ex){
            errorHandler(res, ex)
        }
    })

    app.get('/build/log/:id', async (req, res)=>{
        try {
            const view = await handlebars.getView('buildLog'),
                data = await pluginsManager.getExclusive('dataProvider'),
                build = await data.getBuild(req.params.id, { expected : true }),
                model = {
                    build
                }

            await commonModelHelper(model, req)
            res.send(view(model))
        }catch(ex){
            errorHandler(res, ex)
        }
    })

    app.get('/build/:id', async (req, res)=>{
        try {
            const
                build = await buildLogic.getById(req.params.id),
                job = await jobsLogic.getById(build.jobId),
                logParser = job.logParser ? await pluginsManager.get(job.logParser) : null,
                view = await handlebars.getView('build'),
                model = {
                    build
                }
            
            if (logParser)
                model.build.log = logParser.parseErrors(model.build.log)
            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

