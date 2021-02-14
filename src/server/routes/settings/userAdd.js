const viewModelHelper = require(_$+'helpers/viewModel'),
    errorHandler = require(_$+'helpers/errorHandler'),
    sessionHelper = require(_$+'helpers/session'), 
    handlebars = require(_$ +'helpers/handlebars')

module.exports = function(app){
    app.get('/settings/userAdd', async function(req, res){
        try {
            //////////////////////////////////////////////////////////
            await sessionHelper.ensureRole(req, 'admin')
            //////////////////////////////////////////////////////////

            let 
                view = await handlebars.getView('settings/userAdd'),
                model = { }

            await viewModelHelper.common(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

