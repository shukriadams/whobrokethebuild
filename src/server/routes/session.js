const settings = require(_$+ 'helpers/settings'),
    constants = require(_$+ 'types/constants'),
    errorHandler = require(_$+'helpers/errorHandler'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    sessionLogic = require(_$+'logic/session')
    
module.exports = function(app){

    /**
     * Logs user out (deletes session associated with current user)
     */
    app.delete('/session', async function(req, res){
        try {
            res.clearCookie(constants.COOKIE_AUTHKEY)
            res.json({
                error : null
            })
        } catch(ex){
            errorHandler(res, ex)
        }
    })


    /**
     * Attempts to log a user in (create a session)
     */
    app.post('/session', async function(req, res){
        try {
            const username = req.body.username,
                password = req.body.password

            if (!username || !password){
                
                res.status(400)

                return res.json({
                    code : constants.RESPONSECODES_MISSINGDATA,
                    status : 400,
                    error : 'Username and password are required'
                })
            }

            let authProviders = await pluginsManager.getAllByCategory('authProvider'), 
                authResult = null
                
            // try to log in with all available auth providers
            for (const authProvider of authProviders){
                authResult = await authProvider.processLoginRequest(username, password, req.useragent.source)
                if (authResult.result === constants.LOGINRESULT_SUCCESS)
                    break
            }

            if (authResult.result !== constants.LOGINRESULT_SUCCESS){
                res.status(401)
                return res.json({
                    code : constants.RESPONSECODES_INVALIDCREDENTIALS,
                    status : 401,
                    error : 'invalid username or password'
                })
            }

            if (!authResult.userId){
                res.status(401)
                return res.json({
                    code : constants.RESPONSECODES_INVALIDCREDENTIALS,
                    status : 401,
                    error : 'login passed but external credentials not mapped to internal user'
                })
            }

            let session = await sessionLogic.insert(authResult.userId, req.useragent.source)
            res.cookie(constants.COOKIE_AUTHKEY, session.id, {
                maxAge: 1000 * 60 * 1440 * settings.cookiesDays, // 1440 minutes in day
                httpOnly: true
            })

            return res.json({
                status : constants.RESPONSECODES_LOGINSUCCESS,
                error : null
            })

        } catch(ex){
            errorHandler(res, ex)
        }
    })
}

