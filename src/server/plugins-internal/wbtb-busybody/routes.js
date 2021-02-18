const sessionHelper = require(_$+'helpers/session'),
    handlebars = require(_$+'helpers/handlebars'),
    thisType = 'wbtb-busybody',
    viewModelHelper = require(_$+'helpers/viewModel'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler')

module.exports = app => {

    /**
     * Gets view of settings for a specific user
     */    
    app.get(`/${thisType}/user/:userId`, async function(req, res){
        try {

            //////////////////////////////////////////////////////////
            await sessionHelper.ensureUserOrRole(req, req.params.userId, 'admin')
            //////////////////////////////////////////////////////////
            
            const view = await handlebars.getView(`${thisType}/views/user`),
                data = await pluginsManager.getExclusive('dataProvider'),
                user = await data.getUser(req.params.userId, { expected : true }),
                model = {
                    wbtbBusybody : { 

                    }
                }

            model.wbtbBusybody.alertOnAllBuildErrors = user.pluginSettings[thisType] ? user.pluginSettings[thisType].alertOnAllBuildErrors : false

            await viewModelHelper.layout_userSettings(model, req, req.params.userId)

            res.send(view(model))
        } catch(ex){
            errorHandler(res, ex)
        }
    })

    
    /**
     * Saves settigs for a specific user
     */
    app.post(`/${thisType}/user/:userId`, async function(req, res){
        try {

            //////////////////////////////////////////////////////////
            await sessionHelper.ensureUserOrRole(req, req.params.userId, 'admin')
            //////////////////////////////////////////////////////////

            const data = await pluginsManager.getExclusive('dataProvider'),
                user = await data.getUser(req.params.userId, {expected : true}),
                pluginSetting = user.pluginSettings[thisType] || {}

            user.pluginSettings[thisType] = pluginSetting
            pluginSetting.alertOnAllBuildErrors = req.body.alertOnAllBuildErrors === 'on'

            await data.updateUser(user)

            res.redirect(`/${thisType}/user/${user.id}`)
        } catch(ex){
            errorHandler(res, ex)
        }
    })
}