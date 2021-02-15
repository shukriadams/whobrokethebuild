const viewModelHelper = require(_$+'helpers/viewModel'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+ '/helpers/handlebars')

module.exports = function(app){
    
    /**
     * Renders login view
     */
    app.get('/login', async function(req, res){
        try {
            const view = await handlebars.getView('login'),
                model = {}

            await viewModelHelper.layout(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

