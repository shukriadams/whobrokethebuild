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
            const view = await handlebars.getView('job'),
                model = { },
                data = await pluginsManager.getExclusive('dataProvider'),
                page = parseInt(req.query.page || 1) - 1 // pages are publicly 1-rooted, 0-rooted internally
            
            model.job = await data.getJob(req.params.id, { expected : true })
            model.baseUrl = `/job/${req.params.id}`
            model.jobBuilds = await data.pageBuilds(req.params.id, page, settings.standardPageSize)
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

