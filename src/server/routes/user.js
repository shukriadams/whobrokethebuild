const pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    appendCommonViewModel = require(_$+ 'helpers/appendCommonViewModel'),
    handlebars = require(_$+ 'helpers/handlebars')

module.exports = function(app){
    app.get('/user/:user', async function(req, res){
        try {
            const data = await pluginsManager.getExclusive('dataProvider'),
                view = await handlebars.getView('user'),
                user = await data.getUser(req.params.user, { expected : true }),
                model = {
                    user
                }

            // gets builds user was involved in
            model.buildInvolvements = await data.getBuildInvolvementByUserId(req.params.user)

            // expand related objects
            for (const buildInvolvement of model.buildInvolvements){
                if (!buildInvolvement.__build)
                    continue

                buildInvolvement.__build.__job = await data.getJob(buildInvolvement.__build.jobId)
            }

            await appendCommonViewModel(model, req, req.params.user)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

