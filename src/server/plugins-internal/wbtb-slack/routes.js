
const 
    handlebars = require(_$+'helpers/handlebars'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    PluginSetting = require(_$+'types/pluginSetting'),
    ContactMethod = require(_$+'types/contactMethod'),
    thisType = 'wbtb-slack',
    slackLogic = require('./index'),
    errorHandler = require(_$+'helpers/errorHandler'),
    contactSetting = {
        channels: [] // string array of slack channel ids to broadcast errors to
    };


module.exports = app => {

    app.get('/wbtb-slack/', async function(req, res){
        try {
            const view = await handlebars.getView('wbtb-slack/views/settings'),
                data = await pluginsManager.getExclusive('dataProvider'),
                token = await data.getPluginSetting(thisType, 'token'),
                model = {
                    wbtbSlack : {

                    }
                }

            model.wbtbSlack.accessToken = token ? token.value : null
            model.jobs = await data.getAllJobs()
            // collapse contactMethod to only this plugin's data
            for (let job of model.jobs)
                job.__contactMethod = job.contactMethods[thisType]

            model.channels = await slackLogic.getChannels()
            model.channels.unshift({ id : null, name : 'No channel'})
            res.send(view(model))
        } catch(ex){
            errorHandler(res, ex)
        }
    })

    app.get('/wbtb-slack/user/:userId', async function(req, res){
        try {
            const view = await handlebars.getView('wbtb-slack/views/user')
                data = await pluginsManager.getExclusive('dataProvider'),
                user = await data.getUser(req.params.userId, {expected : true}),
                model = {
                    wbtbSlack : { 
                        user
                    }
                }

            model.wbtbSlack.slackId = user.contactMethods[thisType] ? user.contactMethods[thisType].slackId : null

            res.send(view(model))
        } catch(ex){
            errorHandler(res, ex)
        }
    })

    app.post('/wbtb-slack/settings', async function(req, res){
        try {
            let data = await pluginsManager.getExclusive('dataProvider'),
                token = await data.getPluginSetting(thisType, 'token'),
                jobs = await data.getAllJobs()

            // update or create the token field for this plugin in the generic plugin settings 
            if (token) {
                token.value = req.body.token
                await data.updatePluginSetting(token)
            } else {
                token = PluginSetting()
                token.plugin = thisType
                token.name = 'token'
                token.value = req.body.token
                await data.insertPluginSetting(token)
            }
            
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

            res.redirect('/wbtb-slack')
        } catch(ex){
            errorHandler(res, ex)
        }
    })

    
    app.post('/wbtb-slack/user/:userId', async function(req, res){
        try {
            const data = await pluginsManager.getExclusive('dataProvider'),
                user = await data.getUser(req.params.userId, {expected : true})

            const contactMethod = user.contactMethods[thisType] || {}
            user.contactMethods[thisType] = contactMethod
            contactMethod.slackId = req.body.slackId
            await data.updateUser(user)

            res.redirect(`/wbtb-slack/user/${user.id}`)
        } catch(ex){
            errorHandler(res, ex)
        }
    })

}


