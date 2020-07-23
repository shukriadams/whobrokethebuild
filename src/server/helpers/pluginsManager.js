let
    settings = require(_$+'helpers/settings'),
    path = require('path'),
    fs = require('fs-extra'),
    colors = require('colors/safe'),
    urljoin = require('urljoin'),
    jsonfile = require('jsonfile'),
    Exception = require(_$+ 'types/exception'),
    process = require('process'),
    constants = require(_$+ 'types/constants'),
    fsUtils = require('madscience-fsUtils'),
    exec = require('madscience-node-exec'),
    hash = require(_$+'helpers/hash'),
    logger = require('winston-wrapper').new(settings.logPath),
    _plugins = {}

module.exports = {

    /**
     * gets folders of all plugins on system, AFTER plugin folders have been scaffolded up
     */
    async getPluginsFolders(){
        let installedPlugins = await fsUtils.getChildDirs('./server/plugins')
        if (settings.enableDevPlugins)
            installedPlugins = installedPlugins.concat( await fsUtils.getChildDirs('./server/plugins-dev') )

        return installedPlugins
    },


    async _loadPlugins(){
        
    },


    /**
     * Ensures that all plugins are installed and up-to-date (git clone + npm install on all plugins). For a plugin to be 
     * installed it must be flagged as active
     */
    async initializeAll(){
        const 
            sources = ['git'], // removed npm as option as not yet implemented
            // this is where plugins will normally be installed
            externalPluginsFolder = './server/plugins'
            // read the regular plugins list
            pluginsConfig = await fs.readJson('./plugins.json')

        // if a dev plugin list exists, load and merge that with the regular plugins list, let dev plugins override regular ones
        if (settings.enableDevPlugins && await fs.pathExists('./plugins.local.json')){
            const devPluginsConfig = await fs.readJson('./plugins.local.json')
            // each dev plugin needs a .dev flag so we know to load from plugins-dev folder
            for (const plugin in devPluginsConfig)
                devPluginsConfig[plugin].dev = true

            pluginsConfig = Object.assign(pluginsConfig, devPluginsConfig)
        }
        
        // strip out all plugin config if explicitly disabled 
        let disabledCount = 0
        for (const pluginName in pluginsConfig)
            if (pluginsConfig[pluginName].enabled === false){
                delete pluginsConfig[pluginName]
                disabledCount ++
            }

        if (disabledCount)
            logger.info.info(`${disabledCount} plugin(s) were declared but disabled`)

        // validate plugins
        let errors = false
        for (const pluginName in pluginsConfig){
            const pluginConfig = pluginsConfig[pluginName]

            // dev plugins don't require any config validation
            if (pluginConfig.dev)
                continue

            // .source is required
            if (!pluginConfig.source){
                logger.error.error(`plugin "${pluginName}" config does not define "source"`)
                errors = true
            }

            // .version is required
            if (!pluginConfig.version){
                logger.error.error(`plugin "${pluginName}" config does not define "version"`)
                errors = true
            }

            if (pluginConfig.source && !sources.includes(pluginConfig.source)){
                logger.error.error(`plugin "${pluginName}" source "${pluginConfig.source}" is not supported. Allowed values : "${sources.join(',')}"`)
                errors = true
            }

            if (pluginConfig.source === 'git' && !pluginConfig.url){
                logger.error.error(`plugin "${pluginName}" has source "git" but does not define "url"`)
                errors = true
            }

            if (pluginConfig.source === 'npm' && !pluginConfig.package){
                logger.error.error(`plugin "${pluginName}" has source "npm" but does not define "package"`)
                errors = true
            }

        }

        if (errors){
            logger.error.error(`Setup errors were detected in plugins - Who Broke The Build cannot start`)
            return process.exit(1)
        }


        await fs.ensureDir(externalPluginsFolder)


        // install all external plugins, npm install on all plugins
        for (const pluginName in pluginsConfig){
            let 
                pluginConfig = pluginsConfig[pluginName],
                pluginParentFolder = pluginConfig.dev ? './server/plugins-dev' : './server/plugins',
                pluginFolder = `${path.join(pluginParentFolder, pluginName)}`, 
                // we write out own per-plugin JSON file in root of plugins folder, this contains metadata about the installation
                pluginInstallStatus = {},
                pluginInstallStatusPath = `${path.join(pluginParentFolder, `${pluginName}.json`)}`,
                packageHasErrors = false

            // load plugin status if it exists
            if (await fs.pathExists(pluginInstallStatusPath))
                pluginInstallStatus = await fs.readJson(pluginInstallStatusPath)

            if (!pluginConfig.dev && pluginConfig.source === 'git'){

                // check if installed plugin version matches requested version
                const alreadyInstalled = pluginInstallStatus.url === pluginConfig.url
                    && pluginInstallStatus.version === pluginConfig.version

                if (alreadyInstalled)
                    logger.info.info(`Plugin "${pluginName}" is up to date @ version ${pluginConfig.version}`)
                else {
                    // if plugin is not installed or does not match required version, nuke and reinstall, 
                    // Note : use sync because for some reason async/await trips itself up here
                    if (fs.existsSync(pluginFolder))
                        fs.removeSync(pluginFolder)

                    fs.mkdirSync(pluginFolder)

                    await exec.sh({ 
                        cmd : `git clone --depth 1 --single-branch --branch ${pluginConfig.version} ${pluginConfig.url} ${pluginName}`, 
                        cwd : pluginParentFolder, 
                        // must capture stdio or call never gets released
                        stdio:[0,1,2] 
                    })
                }

                pluginInstallStatus.url = pluginConfig.url
                pluginInstallStatus.version = pluginConfig.version
                pluginInstallStatus.installDate = new Date().getTime()

                console.log(`Plugin "${pluginName}" git cloned from ${pluginConfig.url}`)
            }

            // ensure plugin has package manifest
            const packageManifestPath = path.join(pluginFolder, 'package.json')
            if (!await fs.exists(packageManifestPath)){
                logger.error.error(`Plugin "${pluginName}" does not not have a package.json file`)
                errors = true
                packageHasErrors = true
            }

            if (!packageHasErrors){
                const manifestHash = await hash.file(packageManifestPath)

                if (manifestHash !== pluginInstallStatus.manifestHash){
                    // must cd into folder or npm will fall back to parent app's package.json. use --no-bin-links so we don't entangle
                    // host fs with container fs when running in Docker
                    await exec.sh({ 
                        cmd : `cd ${path.resolve(pluginFolder)} && npm install --no-bin-links`, 
                        stdio:[0,1,2] // pipe this to winston?
                    })

                    pluginInstallStatus.manifestHash = manifestHash
                }
            }
            
            await fs.outputJson(pluginInstallStatusPath, pluginInstallStatus)

        } // foreach plugin


        const installedPluginFolders = await this.getPluginsFolders()

        // bind all discovered plugins
        _plugins = { categoriesAll : {}, categories : { }, plugins: { } }
        for (let installedPlugin of installedPluginFolders){
            const 
                pluginName = path.basename(installedPlugin),
                pluginPackageJsonPath = path.join(installedPlugin,  'package.json'),
                pluginConfig = pluginsConfig[pluginName]

            // plugin folder exists locally, but is not defined in pluginsConfig, ignore it
            if (!pluginConfig)
                continue
                
            if (!await fs.pathExists(pluginPackageJsonPath)){
                console.log(colors.red(`ERROR : "Plugin ${pluginName}" missing package.json`))
                errors = true
                continue
            }

            const packageJson = await fs.readJson(pluginPackageJsonPath);
            if (!packageJson.wbtb){
                logger.error.error(`ERROR : Plugin "${pluginName}" missing package.json:wbtb`)
                errors = true
                continue
            }

            if (!packageJson.wbtb.category){
                logger.error.error(`ERROR : Plugin "${pluginName}" missing package.json:wbtb.category`)
                errors = true
                continue
            }

            _plugins.plugins[pluginName] = installedPlugin
            _plugins.categoriesAll[packageJson.wbtb.category] = _plugins.categoriesAll[packageJson.wbtb.category] || {}
            _plugins.categoriesAll[packageJson.wbtb.category][pluginName] = installedPlugin 

            // try to find plugin in pluginconfig, if found, bind to category as active
            if (pluginConfig){
                
                const overrittenPlugin = _plugins.categories[packageJson.wbtb.category];
                if (overrittenPlugin)
                    console.log(colors.yellow(`WARNING : Plugin "${pluginName}" will overwrite "${overrittenPlugin}" in category "${packageJson.wbtb.category}"`))

                 _plugins.categories[packageJson.wbtb.category] = installedPlugin
            }

            logger.info.info(`Plugin "${pluginName}" loaded`)
        }

        
        if (errors){
            logger.error.error(`One or more plugins have invalid internal configuration - Who Broke The Build cannot start`)
            return process.exit(1)
        }


        // generate ui route index file - some plugins expose a UI
        const pluginConf = {},
            allPlugins = this.getAll()

        for (const plugin of allPlugins){
            if (!plugin.getDescription)
                continue

            const description = plugin.getDescription()
            if (!description)
                throw `a plugin getDescription() returned empty value`
           
            if (description.hasUI && !description.name)
                throw `plugin ${description.id} has a ui but no name` 

            pluginConf[description.id] ={ 
                url : `/${urljoin(description.id)}/`,
                text : description.name
            }
        }

        jsonfile.writeFileSync('./.plugin.conf', pluginConf)
    },


    /**
     * Gets a plugin by category. If the plugin doesn't exist, an exception is thrown
     */
    getByCategory(category){
        const pluginPath = _plugins.categories[category]
        if (!pluginPath)
            throw new Exception({ code : constants.ERROR_MISSINGPLUGIN, public : `Category : ${category}` })
        return require(`${path.resolve(pluginPath)}/index`)
    },

    /**
     * 
    */
    get(name){
        const pluginPath = _plugins.plugins[name]
        if (!pluginPath)
            throw new Exception({ code : constants.ERROR_MISSINGPLUGIN, public : `name : ${name}` })

        return require(`${path.resolve(pluginPath)}/index`)
    },

    getAllByCategory(category){
        const categoriesAll = _plugins.categoriesAll[category]
        if (!categoriesAll)
            throw new Exception({ code : constants.ERROR_MISSINGPLUGIN, public : `Category : ${category}` })

        let plugins = []
        for (let pluginPath in categoriesAll){
            console.log(`${path.resolve(categoriesAll[pluginPath])}/index`);
            plugins.push(require(`${path.resolve(categoriesAll[pluginPath])}/index`))
        }
        
        return plugins
    },


    /**
     * gets all active plugins
     */
    getAll(){
        let plugins = []
        for (let pluginPath in  _plugins.plugins)
            plugins.push(require(`${path.resolve(_plugins.plugins[pluginPath])}/index`))
        
        return plugins
    },


    /**
     * Gets an array of typecodes from all plugins belonging to a given category
     */
    async getTypeCodesOf(category){
        const types = [],
            plugins = await pluginsManager.getAllByCategory(category)

        for (let plugin of plugins)
            types.push(plugin.getTypeCode()) 

        return types
    },

    async validateSettings(){
        const plugins = await this.getAll()
        for(const plugin of plugins){
            const isValid = await plugin.validateSettings()
            if (!isValid)
                throw `Settings validation failed for plugin ${plugin.getDescription().id}`
        }
    },

    async runDiagnostic(){
        
    }
}   