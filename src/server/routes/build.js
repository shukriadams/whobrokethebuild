const appendCommonViewModel = require(_$+'helpers/appendCommonViewModel'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    logHelper = require(_$+'helpers/log'),
    buildLogic = require(_$+'logic/builds'),
    jobsLogic = require(_$+'logic/job'),
    constants = require(_$+'types/constants'),
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
                logHelper = require(_$+'helpers/log'),
                build = await data.getBuild(req.params.id, { expected : true }),
                job = await jobsLogic.getById(build.jobId),
                model = {
                    build
                }

            model.log = await logHelper.parseFromFile(build.logPath, job.logParser)
            await appendCommonViewModel(model, req)
            res.send(view(model))
        } catch(ex) {
            errorHandler(res, ex)
        }
    })

    app.get('/build/:id', async (req, res)=>{
        try {
            const data = await pluginsManager.getExclusive('dataProvider'),
                build = await buildLogic.getById(req.params.id),
                view = await handlebars.getView('build'),
                model = {
                    build
                }

            build.__job = await jobsLogic.getById(build.jobId)
            build.__isFailing = build.status === constants.BUILDSTATUS_FAILED 

            // REFACTOR THIS TO LOGIC LAYER
            build.__buildInvolvements = await data.getBuildInvolementsByBuild(build.id)

            for (const buildInvolvement of build.__buildInvolvements){
                // get user object for revision, if mapped
                if (buildInvolvement.userId)
                    buildInvolvement.__user = await data.getUser(buildInvolvement.userId)

            }


            // parse and filter log if only certain lines should be shown
            const logParser = build.__job.logParser ? await pluginsManager.get(build.__job.logParser) : null,
                allowedTypes = ['error']

            if (logParser){
                const parsedLog = await logHelper.parseFromFile(build.logPath, build.__job.logParser)
                build.__logParsedLines = parsedLog.lines ? parsedLog.lines.filter(line => allowedTypes.includes(line.type)) : []
                build.__isLogParsed = true
            }

            await appendCommonViewModel(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

