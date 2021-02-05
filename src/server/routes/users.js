const pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    appendCommonViewModel = require(_$+ 'helpers/appendCommonViewModel'),
    handlebars = require(_$+ 'helpers/handlebars')

module.exports = function(app){
    app.get('/users', async function(req, res){
        try {
            const data = await pluginsManager.getExclusive('dataProvider'),
                users = await data.getAllUsers(),
                view = await handlebars.getView('users'),
                model = { users }

            await appendCommonViewModel(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

