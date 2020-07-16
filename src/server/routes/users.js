const 
    pluginsManager = require(_$+'helpers/pluginsManager'),
    settings = require(_$+ 'helpers/settings'),
    errorHandler = require(_$+'helpers/errorHandler'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    handlebars = require(_$+ 'helpers/handlebars')

module.exports = function(app){
    app.get('/users', async function(req, res){
        try {
            const 
                data = await pluginsManager.getByCategory('dataProvider'),
                users = await data.getAllUsers(),
                view = await handlebars.getView('users'),
                model = { users }

            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

