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
                data = await pluginsManager.getExclusive('dataProvider'),
                view = await handlebars.getView('build'),
                model = {
                    build
                }

            build.__buildInvolvements = await data.getBuildInvolementsByBuild(build.id)
            // REFACTOR THIS TO LOGIC LAYER
            for (const buildInvolvement of build.__buildInvolvements)
                if (buildInvolvement.userId)
                    // extend 
                    buildInvolvement.__user = await data.getUser(buildInvolvement.userId)
        

            build.__job = await jobsLogic.getById(build.jobId)
            build.__job.__logParser = build.__job.logParser ? await pluginsManager.get(build.__job.logParser) : null
            
            if (build.__job.__logParser)
                model.build.log = build.__job.__logParser.parseErrors(model.build.log)

            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

