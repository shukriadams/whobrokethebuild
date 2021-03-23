const viewModelHelper = require(_$+'helpers/viewModel'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    buildLogic = require(_$+'logic/builds'),
    jobLogic = require(_$+'logic/job'),
    constants = require(_$+'types/constants'),
    handlebars = require(_$+'helpers/handlebars')

module.exports = function(app){
    
    app.delete('/build/object/:id', async function(req, res){
        try {
            await buildLogic.remove(req.params.id)
            res.json({})
        } catch(ex) {
            errorHandler(res, ex)
        }
    })

    app.delete('/build/alerts/:id', async function(req, res){
        try {
            const build = await buildLogic.getBuild(req.params.id),
                job = await jobLogic.getJob(build.jobId, { expected : true })

            for (const contactMethodName in job.contactMethods){
                const contactPlugin = await pluginsManager.get(contactMethodName)
                if (!contactPlugin)
                    continue

                await contactPlugin.deleteGroupAlert(job.contactMethods[contactMethodName], job, build.id)
            }
            
            res.json({})
        } catch(ex) {
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
                build = await buildLogic.getBuild(req.params.id),
                view = await handlebars.getView('build'),
                job = await jobLogic.getJob(build.jobId),
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
            model.buildBreakers = []
            model.canAlertBeUndone = false

            // determine if any contact plugins have sent a group alert for this which can be rolled back
            if (build.delta === constants.BUILDDELTA_CAUSEBREAK )
                for (const contactMethodName in job.contactMethods){
                    const contactPlugin = await pluginsManager.get(contactMethodName)
                    if (!contactPlugin)
                        continue

                    if (contactPlugin.areGroupAlertsDeletable() && await contactPlugin.groupAlertSent(job.contactMethods[contactMethodName], job, build.id) === true){
                        model.canAlertBeUndone = true
                        break
                    }
                }


            if (build.incidentId && build.incidentId !== build.id)
                model.responsibleBreakingBuild = await buildLogic.getBuild(build.incidentId)

            // parse and filter log if only certain lines should be shown
            for (const buildInvolvement of model.build.involvements){
                // get user object for revision, if mapped
                if (buildInvolvement.userId)
                    buildInvolvement.__user = await data.getUser(buildInvolvement.userId)

                if (buildInvolvement.revisionObject){
                    buildInvolvement.__isFault = buildInvolvement.revisionObject ? !!buildInvolvement.revisionObject.files.find(r => !!r.isFault) : false
                    if (buildInvolvement.__isFault && buildInvolvement.__user)
                        model.buildBreakers.push(buildInvolvement.__user)
                }
            }

            await viewModelHelper.layout(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

