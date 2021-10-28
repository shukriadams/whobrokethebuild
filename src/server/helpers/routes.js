module.exports = {

    /**
     * Finds and initializes all Express route files in core folder (/server/routes), internal plugis folder,
     * and/or external (live) plugings folder if applicable based on settings. Route init requires that Express
     * app instance be passed to the route file, which is responsible for internally binding to app.
     * 
     * @param {object} expressApplication Express application
     */
    async initialize (expressApplication){

        let path = require('path'),
            glob = require('glob'),
            process = require('process'),
            fsUtils = require('madscience-fsUtils'),
            settings = require(_$+ 'helpers/settings'),
            pluginsManager = require(_$+'helpers/pluginsManager'),
            cwd = process.cwd(),
            routeFiles = []
    
        // load core routes
        routeFiles = routeFiles.concat(await fsUtils.readFilesUnderDir(path.join('./server', 'routes')))
    
        // find all plugins, then routes folder under those
        if (settings.bindInternalPlugins)
            routeFiles = routeFiles.concat(glob.sync(`./server/plugins-internal/*/routes.js`, { ignore : ['**/node_modules/**']}))
        else
            routeFiles = routeFiles.concat(glob.sync(`${pluginsManager.getPluginRootPath()}/**/routes.js`, { ignore : ['**/mock/**', '**/node_modules/**']}))

        
        // load routes from all files in /routes folder. These files must return a function that
        // accepts app as arg. Note that route file with name 'default' is reserved and always bound
        // last, this should contain the route that catches all unbound route names and forces them
        // to our Single Page App root page.
        let defaultRoute
        for (let routeFile of routeFiles){
            
            routeFile = path.resolve(routeFile)

            const match = routeFile.match(/(.*).js/)
            if (!match)
                continue

            // remove js extension
            let name = match.pop(),
                route = null

            try {
                route = require(name)
            } catch (ex){
                // rethrow with more context
                throw `Failed to require load route file ${name}. Path invalid or file is not a CommonJS module. ${ex}`
            }

            if (name === 'default'){
                defaultRoute = route
                continue
            }
    
            route(expressApplication)
        }
    
        // finally, load default route. This must be bound last because its pattern works
        // as a catchall for anything that isn't caught by a more specific fixed pattern.
        if (defaultRoute)
            defaultRoute(expressApplication)            
    }
} 