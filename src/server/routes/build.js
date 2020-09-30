const 
    settings = require(_$+ 'helpers/settings'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    buildLogic = require(_$+'logic/builds'),
    jobsLogic = require(_$+'logic/job'),
    vcServerLogic = require(_$+'logic/VCServer'),
    stringSimilarity = require('string-similarity'),
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
                data = await pluginsManager.getExclusive('dataProvider'),
                build = await buildLogic.getById(req.params.id),
                job = await jobsLogic.getById(build.jobId),
                vcServer = await vcServerLogic.getById(job.VCServerId),
                vcServerPlugin = await pluginsManager.get(vcServer.vcs),
                view = await handlebars.getView('build'),
                model = {
                    build
                }

            build.__job = await jobsLogic.getById(build.jobId)
            
            
            let highestFaultChance = 0

            // REFACTOR THIS TO LOGIC LAYER
            build.__buildInvolvements = await data.getBuildInvolementsByBuild(build.id)

            for (const buildInvolvement of build.__buildInvolvements){
                // get revision for a given buildinvolment from source control
                buildInvolvement.__revision = await vcServerPlugin.getRevision(buildInvolvement.revision, vcServer) 

                // determine which revision files were mostl likely involved in build failure - we do this by
                // simply looking for which file path occurred in build log text - really quick+dirty
                for (const file of buildInvolvement.__revision.files){
                    file.__faultChance = stringSimilarity.compareTwoStrings(file.file, model.build.log) 
                    if (file.__faultChance > highestFaultChance)
                        highestFaultChance = file.__faultChance
                }
                    
                // get user object for revision, if mapped
                if (buildInvolvement.userId)
                    buildInvolvement.__user = await data.getUser(buildInvolvement.userId)
            }

            // flag highest fault, for both file and for revision
            for (const buildInvolvement of build.__buildInvolvements)
                for (const file of buildInvolvement.__revision.files)
                    if (file.__faultChance === highestFaultChance){
                        file.__isFault = true
                        buildInvolvement.__isFault = true
                    }
           

            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

