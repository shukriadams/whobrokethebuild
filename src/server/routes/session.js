const 
    settings = require(_$+ 'helpers/settings'),
    constants = require(_$+ 'types/constants'),
    errorHandler = require(_$+'helpers/errorHandler'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    commonModelHelper = require(_$+ 'helpers/commonModels')
    
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
            const 
                username = req.body.username,
                password = req.body.password

            if (!username || !password){
                
                res.status(400)

                return res.json({
                    code : constants.RESPONSECODES_MISSINGDATA,
                    status : 400,
                    error : 'Username and password are required'
                })
            }

            const 
                auth = await pluginsManager.getExclusive('authProvider'), 
                authResult = await auth.processLoginRequest(username, password, req.useragent.source)
                
            if (authResult.result !== constants.LOGINRESULT_SUCCESS){
                res.status(401)
                return res.json({
                    code : constants.RESPONSECODES_INVALIDCREDENTIALS,
                    status : 401,
                    error : 'invalid username or password'
                })
            }

            res.cookie(constants.COOKIE_AUTHKEY, authResult.sessionKey, {
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

