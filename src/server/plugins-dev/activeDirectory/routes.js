
const 
    handlebars = require(_$+'helpers/handlebars'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    logic = require('./index'),
    authMethod = 'ActiveDirectory',
    errorHandler = require(_$+'helpers/errorHandler')

module.exports = function(app){
    
    app.get('/activeDirectory', async function(req, res){
        try {
            const view = await handlebars.getView('activeDirectory/views/settings')
            res.send(view())
        } catch(ex){
            errorHandler(res, ex)
        }
    })

    app.get('/activeDirectory/users', async function(req, res){
        try {
            const data = await pluginsManager.getByCategory('dataProvider'),
                view = await handlebars.getView('activeDirectory/views/users'),
                localUsers = await data.getAllUsers(),
                users = await logic.getAllRemoteUsers()

            for (const user of users)
                if (!!localUsers.find(localUser => user.mail === localUser.publicId))
                    user.isImported = 'on'

            res.send(view({ users }))
        } catch(ex){
            errorHandler(res, ex)
        }
    })

    app.post('/activeDirectory/users', async function(req, res){
        try {
            const data = await pluginsManager.getByCategory('dataProvider'),
                User = require(_$+'types/user'),
                users = await logic.getAllRemoteUsers()

            for (let user of users){
                if (req.body[`cb_userImport_${user.guid}`] === 'on'){
                    let newUser = await data.getByPublicId(user.mail, authMethod)
                    if (!newUser){
                        newUser = User()
                        newUser.publicId = user.mail
                        newUser.name = user.name
                        newUser.email = user.mail
                        newUser.authMethod = authMethod

                        await data.insertUser(newUser)
                        console.log(`added user ${user.name}`)
                    }
                } else {
                    const existingUser = await data.getByPublicId(user.mail, authMethod)
                    if (existingUser){
                        await data.removeUser(existingUser.id)
                        console.log(`removed user ${existingUser.name}`)
                    }
                }

            }

            res.redirect('/activeDirectory/users')
        } catch(ex){
            errorHandler(res, ex)
        }
    })

}

