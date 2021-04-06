/**
 * Starts the WBTB server.
 * 
 * The server in this case is several subsystems which must be initialized. These subsystems are:
 * - 
 */
// set shortcut global for easier module imports relative to server root directory
global._$ = `${__dirname}/server/`

// load first to speed up app loads
require('cache-require-paths')

const Stopwatch = require('statman-stopwatch'),
    stopwatch = new Stopwatch()

stopwatch.start();


(async function(){
    const settings = require(_$+ 'helpers/settings'),
        fs = require('fs-extra')

    // bind log first, this will be globally referenced
    global.__log = require('winston-wrapper').new(settings.logPath, settings.logLevel).log

    // need to do this before we start requiring other componens, as these will often need to write to log folder at start
    await fs.ensureDir(settings.dataFolder)
    await fs.ensureDir(settings.logPath)

    const colors = require('colors/safe'),
        bodyParser = require('body-parser'),
        cookieParser = require('cookie-parser'),
        http = require('http'),
        useragent = require('express-useragent'),
        Express = require('express'),
        daemonManager = require(_$+'daemon/manager'),
        encryption = require(_$+'helpers/encryption'),
        exec = require('madscience-node-exec'),
        diagnosticsHelper = require(_$+'helpers/diagnostics'),
        routeHelper = require(_$+'helpers/routes')
        userLogic = require(_$+ 'logic/users'),
        pluginsManager = require(_$+'helpers/pluginsManager')

    try {


        // run start cmd
        if (await fs.exists('./onStart.sh')){
            console.log('onstart command executing')
            const cmd = await fs.readFile('./onStart.sh')
            try {
                const result = await exec.sh({ cmd })
                console.log(`onstart finished with result`, result)
            } catch(ex){
                console.log(`onstart failed with`, ex)
                if (!settings.ignoreOnStartErrors)
                    process.exit(1)
            }
        }

        // ensure that all required plugins are installed and up-to-date
        await pluginsManager.initialize()

        // run plugin diagnostics mode
        if (settings.runDiagnosticOnStart)
            await diagnosticsHelper.run()

        await pluginsManager.validateSettings()



        const data = await pluginsManager.getExclusive('dataProvider')
        await data.initialize()
        await userLogic.initializeAdmin()
        
        daemonManager.initialize()
        await encryption.testKey()

        // middleware must be loaded before routes
        const app = Express()
        app.use(bodyParser.urlencoded({ extended: false }))
        app.use(bodyParser.json())
        app.use(cookieParser())
        app.use(useragent.express())
        
        // there are two static folders - client is for single page app, public is 
        // for direct express files
        // static folders must be defined before routes or routes will override them
        app.use(Express.static('./client')) // for dev only, not available on builds
        app.use(Express.static('./public'))


        await routeHelper.initialize(app)

        const server = http.createServer(app)
        server.listen(settings.port)

        console.log(colors.green(`Who Broke The Build started, listening on port ${settings.port}`))    
        console.log(`started in ${stopwatch.read()}`)
        console.log(`Log level: ${settings.logLevel}`)
    }catch(ex){
        console.log(colors.red(`ERROR - Who Broke The Build failed to start`))
        console.log(colors.red(ex))
    }
    
})()
