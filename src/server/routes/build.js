const viewModelHelper = require(_$+'helpers/viewModel'),
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
            await viewModelHelper.layout(model, req)
            res.send(view(model))
        } catch(ex) {
            errorHandler(res, ex)
        }
    })

    app.get('/build/:id', async (req, res)=>{
        try {
            const faultHelper = require(_$+'helpers/fault'),
                data = await pluginsManager.getExclusive('dataProvider'),
                build = await buildLogic.getById(req.params.id),
                view = await handlebars.getView('build'),
                job = await jobsLogic.getById(build.jobId),
                vcServer = await data.getVCServer(job.VCServerId, { expected : true }),
                ciServer = await data.getCIServer(job.CIServerId, { expected : true }),
                ciServerPlugin = await pluginsManager.get(ciServer.type),
                vcsPlugin = await pluginsManager.get(vcServer.vcs),
                model = {
                    build,
                    job,
                    ciServer
                }

            model.linkToBuild = ciServerPlugin.linkToBuild(ciServer, job, build)
            build.__isFailing = build.status === constants.BUILDSTATUS_FAILED 

            // REFACTOR THIS TO LOGIC LAYER
            model.buildInvolvements = await data.getBuildInvolementsByBuild(build.id)
            
            // parse and filter log if only certain lines should be shown
            let logParser = model.job.logParser ? await pluginsManager.get(model.job.logParser) : null,
                logErrors = null,
                allowedTypes = ['error']


            if (logParser){
                const parsedLog = await logHelper.parseFromFile(build.logPath, model.job.logParser)
                model.logParsedLines = parsedLog.lines ? parsedLog.lines.filter(line => allowedTypes.includes(line.type)) : []
                model.isLogParsed = true
                logErrors = await logHelper.parseErrorsFromFile(build.logPath, model.job.logParser)
            }

            for (const buildInvolvement of model.buildInvolvements){
                // get user object for revision, if mapped
                if (buildInvolvement.userId)
                    buildInvolvement.__user = await data.getUser(buildInvolvement.userId)

                buildInvolvement.__revision = await vcsPlugin.getRevision(buildInvolvement.revision, vcServer)
                if (logErrors)
                    faultHelper.processRevision(buildInvolvement.__revision, logErrors)
            }




            await viewModelHelper.layout(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

