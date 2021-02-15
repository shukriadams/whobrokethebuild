const pluginsManager = require(_$+'helpers/pluginsManager'),
    viewModelHelper = require(_$+'helpers/viewModel'),
    sessionHelper = require(_$+'helpers/session'), 
    errorHandler = require(_$+'helpers/errorHandler'),
    handlebars = require(_$+'helpers/handlebars')

module.exports = function(app){

    
    app.get('/settings/user/:user', async function(req, res){
        try {

            //////////////////////////////////////////////////////////
            await sessionHelper.ensureUserOrRole(req, req.params.user, 'admin')
            //////////////////////////////////////////////////////////

            const data = await pluginsManager.getExclusive('dataProvider'),
                view = await handlebars.getView('settings/user'),
                user = await data.getUser(req.params.user),
                vcServers  = await data.getAllVCServers(),
                plugins = await pluginsManager.getAllWithUserUI(),
                model = {
                    vcServers,
                    plugins : [],
                    user 
                }
            
            // add links to contact methods that have UI's
            for(const plugin of plugins)
                model.plugins.push({
                    link : `/${plugin.__wbtb.id}/user/${user.id}`,
                    name : plugin.__wbtb.name,
                })
            

            // add vcserver names
            for (const mapping of user.userMappings){
                const vcServer = await data.getVCServer(mapping.VCServerId)
                if (!vcServer)  
                    continue

                mapping.vcServer = vcServer
            }

            await viewModelHelper.layout(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })

    
    app.post('/settings/user/updateVCServerMappings', async function(req, res){
        try {
            //////////////////////////////////////////////////////////
            await sessionHelper.ensureUserOrRole(req, req.body.user, 'admin')
            //////////////////////////////////////////////////////////

            const data = await pluginsManager.getExclusive('dataProvider'),
                user = await data.getUser(req.body.user)

            if (!user)
                throw `user ${req.body.user} not found`

            for(const item of req.body.items){
                const mapping = user.userMappings.find(mapping => mapping.VCServerId === item.VCServerId) 
                if (!mapping)
                    continue // mapping must always be created in add flow below
                
                mapping.name = item.name.trim()
            }

            await data.updateUser(user)
            res.json({ })

        } catch (ex){
            errorHandler(res, ex)
        }
    })


    app.post('/settings/userSettings/addVCServerMapping', async function(req, res){
        try {
            //////////////////////////////////////////////////////////
            await sessionHelper.ensureUserOrRole(req, req.body.user, 'admin')
            //////////////////////////////////////////////////////////

            const data = await pluginsManager.getExclusive('dataProvider'),
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

