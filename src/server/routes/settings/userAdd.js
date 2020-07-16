const 
    settings = require(_$ +'helpers/settings'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$ +'helpers/handlebars')

module.exports = function(app){
    app.get('/settings/userAdd', async function(req, res){
        try {
            let 
                view = await handlebars.getView('settings/userAdd'),
                model = { }

            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

