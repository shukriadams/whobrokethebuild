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
        path = require('path'),
        glob = require('glob'),
        http = require('http'),
        useragent = require('express-useragent'),
        fsUtils = require('madscience-fsUtils'),
        Express = require('express'),
        app = Express(),
        daemonManager = require(_$+'daemon/manager'),
        encryption = require(_$+'helpers/encryption'),
        exec = require('madscience-node-exec'),
        diagnosticsHelper = require(_$+'helpers/diagnostics'),
        userLogic = require(_$+ 'logic/users'),
        pluginsManager = require(_$+'helpers/pluginsManager')

    try {
        // allow user to run start cmd
        if (await fs.exists('./onStart.sh')){
            console.log('onstart command executing')
            const cmd = await fs.readFile('./onStart.sh')
            try {
                const result = await exec.sh({ cmd })
                console.log(`onstart finished with result`, result)
            } catch(ex){
                console.log(`onstart failed with`, ex)
                process.exit(1)
            }
        }

        // ensure that all required plugins are installed and up-to-date
        await pluginsManager.initializeAll()

        // run plugibn diagnostics mode
        if (settings.runDiagnosticOnStart){
            await pluginsManager.runDiagnostic()
            await diagnosticsHelper.run()
        }

        await pluginsManager.validateSettings()

        let root = path.join(__dirname, 'server'),
            routeFiles = [],
            data = await pluginsManager.getExclusive('dataProvider')

        // load core routes
        routeFiles = routeFiles.concat(await fsUtils.readFilesUnderDir(path.join(root, 'routes')))

        // find all plugins, then routes folder under those
        if (settings.bindInternalPlugins)
            routeFiles = routeFiles.concat(glob.sync(`${root}/plugins-internal/*/routes.js`, { ignore : ['**/node_modules/**']}))
        else
            routeFiles = routeFiles.concat(glob.sync(`${root}/plugins/*/routes.js`, { ignore : ['**/node_modules/**']}))

        await data.initialize()
        await userLogic.initializeAdmin()
        
        daemonManager.startAll()
        await encryption.testKey()

        // middleware must be loaded before routes
        app.use(bodyParser.urlencoded({ extended: false }))
        app.use(bodyParser.json())
        app.use(cookieParser())
        app.use(useragent.express())
        
        // there are two static folders - client is for single page app, public is 
        // for direct express files
        // static folders must be defined before routes or routes will override them
        app.use(Express.static('./client')) // for dev only, not available on builds
        app.use(Express.static('./public'))
        
        // load routes from all files in /routes folder. These files must return a function that
        // accepts app as arg. Note that route file with name 'default' is reserved and always bound
        // last, this should contain the route that catches all unbound route names and forces them
        // to our Single Page App root page.
        let defaultRoute
        
        for (let routeFile of routeFiles){

            routeFile = routeFile.replace(root,'')
            const match = routeFile.match(/(.*).js/)
            if (!match)
                continue

            const name = match.pop()
    
            let routes = require(`./server/${name}`)
            if (name === 'default'){
                defaultRoute = routes
                continue
            }
    
            routes(app)
        }
    
        // finally, load default route. This must be bound last because its pattern works
        // as a catchall for anything that isn't caught by a more specific fixed pattern.
        if (defaultRoute)
            defaultRoute(app)

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
