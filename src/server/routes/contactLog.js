const 
    pluginsManager = require(_$+'helpers/pluginsManager'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+'helpers/handlebars')

module.exports = function(app){
    
    app.get('/contactLog/:page?', async function(req, res){
        try {
            const 
                view = await handlebars.getView('contactLog'),
                model = { },
                data = await pluginsManager.getByCategory('dataProvider'),
                page = parseInt(req.query.page || 1) - 1 // pages are publicly 1-rooted, 0-rooted internally
            
            model.contactLogs = await data.pageContactLogs(page, settings.standardPageSize)
            for (const contactLog of model.contactLogs.items)
                contactLog.__user = await data.getUser(contactLog.userId)

            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })

}

