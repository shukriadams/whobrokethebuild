const pluginsManager = require(_$+'helpers/pluginsManager'),
    errorHandler = require(_$+'helpers/errorHandler'),
    viewModelHelper = require(_$+'helpers/viewModel'),
    handlebars = require(_$+ 'helpers/handlebars')

module.exports = function(app){
    app.get('/users', async function(req, res){
        try {
            let data = await pluginsManager.getExclusive('dataProvider'),
                users = await data.getAllUsers(),
                view = await handlebars.getView('users'),
                model = { }

            users = users.sort((a, b)=>{
                const aName = a.name ? a.name.toLowerCase() : '',
                    bName = b.name ? b.name.toLowerCase() : ''

                return aName.toLowerCase() > bName.toLowerCase() ? 1 :
                    bName.toLowerCase() > aName.toLowerCase() ? -1 :
                    0
            })

            // map usermappings on vcservers
            for (const user of users){
                if (!user.userMappings)
                    continue

                for (let mapping of user.userMappings){
                    const vcServer = await data.getVCServer(mapping.VCServerId)
                    if (!vcServer)
                        mapping.__error = `Mapped to invalid vcserver ${mapping.VCServerId}`
                    
                    mapping.__vcServerBinding = vcServer
                }
            }

            model.users = users
            await viewModelHelper.layout(model, req)
            res.send(view(model))

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

