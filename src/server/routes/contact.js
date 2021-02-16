const errorHandler = require(_$+'helpers/errorHandler'),
    pluginsManager = require(_$+'helpers/pluginsManager'),
    sessionHelper = require(_$+'helpers/session') 

module.exports = app =>{

    /**
     * Sends a test slack message for the given user, for an error in the given reference log. Use this for
     * testing slack message formatting and integration.
     */ 
    app.get('/contact/slack/:refLogPath', async(req, res)=>{
        try {

            //////////////////////////////////////////////////////////
            await sessionHelper.ensureRole(req, 'admin')
            //////////////////////////////////////////////////////////

            let settings = require(_$+ 'helpers/settings'),
                constants = require(_$+ 'types/constants'),
                fs = require('fs-extra'),
                path = require('path'),
                data = await pluginsManager.getExclusive('dataProvider'),
                user = await sessionHelper.getCurrentUser(req),
                build = (await data.getAllBuilds()).slice(0,1)
            
            if (!build.length)
                return res.end('No build detected in system')

            build = build[0]

            // force build to fail
            build.status = constants.BUILDSTATUS_FAILED

            if (!user)
                return res.end(`Userid ${req.params.userId} is not valid`)

            const logPath = path.join(settings.buildLogsDump, req.params.refLogPath)
            if (!await fs.exists(logPath))
                return res.end(`count not find reference log ${req.params.refLogPath}`)

            const slack = pluginsManager.get('wbtb-slack')
            slack.__wbtb.sandboxMode = false // false this to realsies
            await slack.alertUser(user, build, 'implicated', true)

            res.end('user notified')

        } catch(ex){
            errorHandler(res, ex)
        }
    })

}
