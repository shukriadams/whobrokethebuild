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
            model.buildInvolvements = await data.pageBuildInvolvementByUser(req.params.user, page, settings.standardPageSize)
            model.baseUrl = `/user/${req.params.user}`

            // expand related objects
            for (const buildInvolvement of model.buildInvolvements.items){
                if (!buildInvolvement.__build)
                    continue

                buildInvolvement.__build.__job = await data.getJob(buildInvolvement.__build.jobId)
            }

            await viewModelHelper.common(model, req, req.params.user)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

