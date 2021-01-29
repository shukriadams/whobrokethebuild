const 
    settings = require(_$+'helpers/settings'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    commonModelHelper = require(_$+ 'helpers/commonModels'),
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+'helpers/handlebars')

module.exports = function(app){
    app.get('/settings/user/:user', async function(req, res){
        try {
            const 
                data = await pluginsManager.getExclusive('dataProvider'),
                view = await handlebars.getView('settings/user'),
                user = await data.getUser(req.params.user),
                vcServers  = await data.getAllVCServers(),
                contactMethods = await pluginsManager.getAllByCategory('contactMethod'),
                model = {
                    vcServers,
                    contactMethods : [],
                    user 
                }
            
            // build contact methods
            for(const contactMethod of contactMethods)
                model.contactMethods.push({
                    link : `/${contactMethod.__wbtb.id}/user/${user.id}`,
                    name : contactMethod.__wbtb.name,
                })

            // add vcserver names
            for (const mapping of user.userMappings){
                const vcServer = await data.getVCServer(mapping.VCServerId)
                if (!vcServer)  
                    continue

                mapping.vcServer = vcServer
            }

            await commonModelHelper(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })

    
    app.post('/settings/user/updateVCServerMappings', async function(req, res){
        try {
            const 
                data = await pluginsManager.getExclusive('dataProvider'),
                user = await data.getUser(req.body.user)

            if (!user)
                throw `user ${req.body.user} not found`

            for(const item of req.body.items){
                const mapping = user.userMappings.find(mapping => mapping.VCServerId === item.VCServerId) 
                if (!mapping)
                    continue // mapping must always be created in add flow below
                
                mapping.name = item.name
            }

            await data.updateUser(user)
            res.json({ })

        } catch (ex){
            errorHandler(res, ex)
        }
    })


    app.post('/settings/userSettings/addVCServerMapping', async function(req, res){
        try {
            const 
                data = await pluginsManager.getExclusive('dataProvider'),
                user = await data.getUser(req.body.user)

            if(!user)
                return res.json({
                    error : 'user not found'
                })
            
            let userMapping = user.userMappings.find(mapping => mapping.VCServerId === req.body.vcServer)
            if (!userMapping){
                userMapping = {
                    VCServerId : req.body.vcServer,
                    name: ''
                }

                user.userMappings.push(userMapping)
            }

            await data.updateUser(user)
            res.json({error : null})

        } catch(ex){
            errorHandler(res, ex)
        }
    })

    
}

