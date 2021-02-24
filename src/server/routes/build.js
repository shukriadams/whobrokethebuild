const viewModelHelper = require(_$+'helpers/viewModel'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
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
                model = {
                    build
                }

            model.log = await logHelper.readRawLogForBuild(build)
            await viewModelHelper.layout(model, req)
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
                job = await jobsLogic.getById(build.jobId),
                vcServer = await data.getVCServer(job.VCServerId, { expected : true }),
                ciServer = await data.getCIServer(job.CIServerId, { expected : true }),
                ciServerPlugin = await pluginsManager.get(ciServer.type),
                model = {
                    build,
                    job,
                    ciServer
                }

            model.nextBuild = await data.getNextBuild(build)
            model.previousBuild = await data.getPreviousBuild(build)
            model.linkToBuild = ciServerPlugin.linkToBuild(ciServer, job, build)
            build.__isFailing = build.status === constants.BUILDSTATUS_FAILED 

            if (build.incidentId && build.incidentId !== build.id)
                model.responsibleBreakingBuild = await buildLogic.getById(build.incidentId)

            // parse and filter log if only certain lines should be shown
            for (const buildInvolvement of model.build.involvements){
                // get user object for revision, if mapped
                if (buildInvolvement.userId)
                    buildInvolvement.__user = await data.getUser(buildInvolvement.userId)

                if (buildInvolvement.revisionObject)
                   buildInvolvement.__isFault = buildInvolvement.revisionObject ? !!buildInvolvement.revisionObject.files.find(r => !!r.isFault) : false
            }

            await viewModelHelper.layout(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

