const pluginsManager = require(_$+'helpers/pluginsManager'),
    settings = require(_$+ 'helpers/settings'),
    viewModelHelper = require(_$+'helpers/viewModel'),
    jobLogic = require(_$+'logic/job'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+'helpers/handlebars')

module.exports = function(app){

    app.delete('/jobs/:id', async(req, res)=>{
        try {
            await jobLogic.delete(req.params.id)
            res.json({})
        } catch(ex) {
            errorHandler(res, ex)
        }
    })

    app.get('/job/:id?', async function(req, res){
        try {
            const userLogic = require(_$+'logic/users'),
                view = await handlebars.getView('job'),
                faultHelper = require(_$+'helpers/fault'),
                model = { },
                data = await pluginsManager.getExclusive('dataProvider'),
                page = parseInt(req.query.page || 1) - 1 // pages are publicly 1-rooted, 0-rooted internally
            
            model.job = await jobLogic.getJob(req.params.id, { expected : true })
            model.baseUrl = `/job/${req.params.id}`
            model.jobBuilds = await data.pageBuilds(req.params.id, page, settings.standardPageSize)
            model.brokenByUsers = []
            model.brokenSince = null
            model.buildThatBrokeJob = await data.getBuildThatBrokeJob(model.job.id)

            if (model.buildThatBrokeJob){
                let brokenBy = []

                model.brokenSince = model.buildThatBrokeJob.ended
                brokenBy = await faultHelper.getUsersWhoBrokeBuild(model.buildThatBrokeJob)
    
                for (const userId of brokenBy){
                    const user = await userLogic.getByExternalName(model.job.VCServerId, userId)
                    if (user)
                        model.brokenByUsers.push(user)
                }
            }

            for (const build of model.jobBuilds.items)
                build.involvements = build.involvements.sort((a,b)=>{
                    const arev = a.revision.toString(),
                        brev = b.revision.toString()

                    return arev > brev ? -1 :
                        brev > arev ? 1 :
                        0
                })

            await viewModelHelper.layout(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })

}

