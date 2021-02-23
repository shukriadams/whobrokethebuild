const pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    viewModelHelper = require(_$+'helpers/viewModel'),
    handlebars = require(_$+ 'helpers/handlebars')

module.exports = function(app){
    app.get('/user/:user', async function(req, res){
        try {
            const settings = require(_$+'helpers/settings'),
                data = await pluginsManager.getExclusive('dataProvider'),
                view = await handlebars.getView('user'),
                user = await data.getUser(req.params.user, { expected : true }),
                page = parseInt(req.query.page || 1) - 1, // pages are publicly 1-rooted, 0-rooted internally
                model = {
                    user
                }

            // gets builds user was involved in
            model.builds = await data.pageBuildsByUser(req.params.user, page, settings.standardPageSize)
            model.baseUrl = `/user/${req.params.user}`

            // expand related objects
            for (const build of model.builds)
                build.__job = await data.getJob(build.jobId)

            await viewModelHelper.layout(model, req, req.params.user)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

