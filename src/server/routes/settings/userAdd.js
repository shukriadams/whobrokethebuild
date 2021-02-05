const appendCommonViewModel = require(_$+ 'helpers/appendCommonViewModel'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$ +'helpers/handlebars')

module.exports = function(app){
    app.get('/settings/userAdd', async function(req, res){
        try {
            let 
                view = await handlebars.getView('settings/userAdd'),
                model = { }

            await appendCommonViewModel(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

