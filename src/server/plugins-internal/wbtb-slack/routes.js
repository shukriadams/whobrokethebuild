
const handlebars = require(_$+'helpers/handlebars'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    viewModelHelper = require(_$+'helpers/viewModel'),
    sessionHelper = require(_$+'helpers/session'), 
    thisType = 'wbtb-slack',
    slackLogic = require('./index'),
    errorHandler = require(_$+'helpers/errorHandler')

module.exports = app => {

    
    /**
     * Gets view of global slack settings
     */
    app.get(`/${thisType}/`, async function(req, res){
        try {
            //////////////////////////////////////////////////////////
            await sessionHelper.ensureRole(req, 'admin')
            //////////////////////////////////////////////////////////

            const view = await handlebars.getView(`${thisType}/views/settings`),
                data = await pluginsManager.getExclusive('dataProvider'),
                model = {
                    wbtbSlack : {

                    }
                }

            model.jobs = await data.getAllJobs()

            // collapse contactMethod to only this plugin's data
            for (let job of model.jobs)
                job.__contactMethod = job.contactMethods[thisType]

            model.channels = await slackLogic.getChannels()
            model.channels.unshift({ id : null, name : 'No channel'})
            
            await viewModelHelper.layout(model, req)

            res.send(view(model))
        } catch(ex){
            errorHandler(res, ex)
        }
    })


    /**
     * Updates 
     */
    app.post(`/${thisType}/settings`, async function(req, res){
        try {

            //////////////////////////////////////////////////////////
            await sessionHelper.ensureRole(req, 'admin')
            //////////////////////////////////////////////////////////

            let data = await pluginsManager.getExclusive('dataProvider'),
                jobs = await data.getAllJobs()
            
            // save job-channel bindings
            for (const job of jobs){
                const bindToChannelId = req.body[`select_${job.id}`]
                
                if (bindToChannelId)
                    job.contactMethods[thisType] = {
                        channelId : bindToChannelId
                    }
                else
                    delete job.contactMethods[thisType]

                await data.updateJob(job)
            }

            res.redirect(`/${thisType}`)
        } catch(ex){
            errorHandler(res, ex)
        }
    })


    /**
     * Gets view of slack settings for a specific user
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
                    wbtbSlack : { 
                        user
                    }
                }

            model.wbtbSlack.slackId = user.pluginSettings[thisType] ? user.pluginSettings[thisType].slackId : null

            await viewModelHelper.layout_userSettings(model, req, req.params.userId)

            res.send(view(model))
        } catch(ex){
            errorHandler(res, ex)
        }
    })

    
    /**
     * 
     */
    app.post(`/${thisType}/user/:userId`, async function(req, res){
        try {

            //////////////////////////////////////////////////////////
            await sessionHelper.ensureUserOrRole(req, req.params.userId, 'admin')
            //////////////////////////////////////////////////////////

            const data = await pluginsManager.getExclusive('dataProvider'),
                user = await data.getUser(req.params.userId, {expected : true}),
                contactMethod = user.pluginSettings[thisType] || {}

            user.pluginSettings[thisType] = contactMethod
            contactMethod.slackId = req.body.slackId
            await data.updateUser(user)

            res.redirect(`/${thisType}/user/${user.id}`)
        } catch(ex){
            errorHandler(res, ex)
        }
    })


    /**
     * 
     */
    app.get(`/${thisType}/test-alertUser/:userId/:buildId`, async function(req, res){
        try {

            //////////////////////////////////////////////////////////
            await sessionHelper.ensureRole(req, 'admin')
            //////////////////////////////////////////////////////////

            let slackPlugin = await pluginsManager.get('wbtb-slack'),   
                data = await pluginsManager.getExclusive('dataProvider'),
                user = null, 
                build = null
                
            try {
                user = await data.getUser(req.params.userId, { expected : true })
            } catch(ex) {
                res.send(`user not found`)
            }

            try {
                build = await data.getBuild(req.params.buildId, { expected : true })
            } catch(ex) {
                res.send(`build not found`)
            }

            await slackPlugin.alertUser(user, build)

            res.send('user has been contacted')
        } catch(ex){
            errorHandler(res, ex)
        }
    })

}


