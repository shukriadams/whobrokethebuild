const 
    settings = require(_$+ 'helpers/settings'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    handlebars = require(_$+ 'helpers/handlebars')

module.exports = function(app){
    app.get('/user/:user', async function(req, res){
        try {
            const 
                data = await pluginsManager.getByCategory('dataProvider'),
                view = await handlebars.getView('user'),
                user = await data.getUserById(req.params.user),
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

            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

