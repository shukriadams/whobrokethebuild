const 
    settings = require(_$+ 'helpers/settings'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+ 'helpers/handlebars');

module.exports = function(app){
    
    app.delete('/build/:id', async function(req, res){
        try {

            const data = await pluginsManager.getByCategory('dataProvider')
            await data.removeBuild(req.params.id)
            res.json({})
        }catch(ex){
            errorHandler(res, ex)
        }
    })

    app.get('/build/log/:id', async (req, res)=>{
        try {
            const view = await handlebars.getView('buildLog'),
                data = await pluginsManager.getByCategory('dataProvider'),
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
                data = await pluginsManager.getByCategory('dataProvider'),
                build = await data.getBuild(req.params.id, { expected : true }),
                job = await data.getJob(build.jobId, { expected : true }),
                view = await handlebars.getView('build'),
                model = {
                    build,
                    job
                }

            if (job.logParser){
                // todo : get should internally log error if plugin not found
                const logParserPlugin = await pluginsManager.get(job.logParser)
                if (logParserPlugin){
                    build.log = logParserPlugin.parseErrors(build.log)
                }

            }

            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

