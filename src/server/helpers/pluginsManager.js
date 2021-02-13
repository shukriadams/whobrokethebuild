let settings = require(_$+'helpers/settings'),
    path = require('path'),
    fs = require('fs-extra'),
    urljoin = require('urljoin'),
    Exception = require(_$+ 'types/exception'),
    process = require('process'),
    constants = require(_$+ 'types/constants'),
    fsUtils = require('madscience-fsUtils'),
    exec = require('madscience-node-exec'),
    hash = require(_$+'helpers/hash'),
    exclusiveCategories = ['dataProvider'],
    sources = ['git','internal'],
    requiredCategories = ['dataProvider', 'authProvider'],
    pluginConfPath = path.join(settings.dataFolder, '.plugin.conf'),
    _pluginConf = {},
    _plugins = {
        // hash table of installed plugins paths, mapped by id
        plugins : {},
        // hash table of installed plugins (full data), mapped by id.
        byCategory : {}
    },
    _pluginStructure = {
        contactMethod : [
            'canTransmit',
            'alertGroup',
            'alertUser'
        ],
        vcs : [
            'getRevisionPartialName',
            'parseRawRevision',
            'getRevision'
        ]
    }

// own dir copy function, fs-extra corrupts files on vagrant/windows    
async function copyDirectory(source, target){
    const path = require('path'), 
        sourceFiles = await fsUtils.readFilesUnderDir(source)

    for (let sourceFile of sourceFiles){
        sourceFile = sourceFile.replace(source, '')
        const sourceFileFull = path.join(source, sourceFile),
            targetPath = path.join(target, sourceFile)

        await fs.ensureDir(path.dirname(targetPath))
        fs.createReadStream(sourceFileFull).pipe(fs.createWriteStream(targetPath))
    }
}

function containsCategory(pluginsConfig, category){
    for (const plugin in pluginsConfig)
        if (pluginsConfig[plugin].category === category)
            return true
            
    return false
}

module.exports = {

    /**
     * gets folders of all plugins on system, AFTER plugin folders have been scaffolded up
     */
    async getPluginsFolders(){
        let installedPlugins

        if (settings.bindInternalPlugins){
            installedPlugins =  await fsUtils.getChildDirs('./server/plugins-internal')
            __log.info('Binding internal plugins enabled')
        } else {
            installedPlugins = await fsUtils.getChildDirs('./server/plugins')
        }
 
        return installedPlugins
    },


    async _loadPlugins(){
        
    },


    /**
     * Copies internal plugins to standard plugin folder, this is necessary during early dev only when a lot of 
     * standard plugins are shipped as part of the core project.
     * 
     * 
     */
    async _copyInternalPlugins(pluginsConfig){
        let errors = false
        for (const pluginName in pluginsConfig){
            const pluginConfig = pluginsConfig[pluginName]

            //  
            if (pluginConfig.source !== 'internal')
                continue
            
            const sourceFolder = path.join('./server/plugins-internal', pluginName),
                targetFolder = path.join('./server/plugins', pluginName)
            
            if (!await fs.pathExists(sourceFolder)){
                __log.error(`internal plugin "${pluginName}" does not exist in internal plugin cache`)
                errors = true
                continue
            }

            await copyDirectory(sourceFolder, targetFolder)
        }

        if (errors){
            __log.error('Plugin copy failed, shutting down')
            process.exit(1)
        }
    },

    async _generateRouteIndexData(allPlugins){
        for (const plugin of allPlugins){
            const description = plugin.__wbtb
            if (!description)
                throw `a plugin __wbtb is not set`
           
            if (description.hasUI && !description.name)
                throw `plugin ${description.id} has a ui but no name` 

            _pluginConf[description.id] ={ 
                url : `/${urljoin(description.id)}/`,
                hasUI : description.hasUI === true,
                text : description.name
            }
        }
    },

    async _initializeAllPlugins(allPlugins){
        for (const plugin of allPlugins){
            if (plugin.initialize && typeof plugin.initialize === 'function'){
                try {
                    await plugin.initialize()
                } catch (ex){
                    __log.error(`Error trying to initialize plugin ${plugin.__wbtb} : `, ex)
                }
            }
        }
    },

    /**
     * Validates the contents of pluginconfig. Logs errors out. Shuts app down if validation fails.
     */
    async _ensurePluginsValid(pluginsConfig){
        let errors = false

        for (const pluginName in pluginsConfig){
            const pluginConfig = pluginsConfig[pluginName]

            // dev plugins don't require any config validation
            if (pluginConfig.source === 'internal')
                continue

            // .source is required
            if (!pluginConfig.source){
                __log.error(`plugin "${pluginName}" config does not define "source"`)
                errors = true
            }

            // .version is required
            if (!pluginConfig.version){
                __log.error(`plugin "${pluginName}" config does not define "version"`)
                errors = true
            }

            if (pluginConfig.source && !sources.includes(pluginConfig.source)){
                __log.error(`plugin "${pluginName}" source "${pluginConfig.source}" is not supported. Allowed values : "${sources.join(',')}"`)
                errors = true
            }

            if (pluginConfig.source === 'git' && !pluginConfig.url){
                __log.error(`plugin "${pluginName}" has source "git" but does not define "url"`)
                errors = true
            }

            if (pluginConfig.source === 'npm' && !pluginConfig.package){
                __log.error(`plugin "${pluginName}" has source "npm" but does not define "package"`)
                errors = true
            }
        }

        if (errors){
            __log.error('Plugin validation failed, shutting down')
            process.exit(1)
        }
    },


    /**
     * 
    */
    async _setupAllPlugins(pluginsConfig){
        let errors = true

        for (const pluginName in pluginsConfig){
            let pluginConfig = pluginsConfig[pluginName],
                pluginParentFolder = settings.bindInternalPlugins ? './server/plugins-internal' :  './server/plugins',
                pluginFolder = `${path.join(pluginParentFolder, pluginName)}`, 
                // we write out own per-plugin JSON file in root of plugins folder, this contains metadata about the installation
                pluginInstallStatus = {},
                pluginInstallStatusPath = `${path.join(pluginParentFolder, `${pluginName}.json`)}`,
                packageHasErrors = false

            // load plugin status if it exists
            if (await fs.pathExists(pluginInstallStatusPath))
                pluginInstallStatus = await fs.readJson(pluginInstallStatusPath)

            if (pluginConfig.source === 'git'){

                // check if installed plugin version matches requested version
                const alreadyInstalled = pluginInstallStatus.url === pluginConfig.url
                    && pluginInstallStatus.version === pluginConfig.version

                if (alreadyInstalled)
                    __log.info(`Plugin "${pluginName}" is up to date @ version ${pluginConfig.version}`)
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

                __log.info(`Plugin "${pluginName}" git cloned from ${pluginConfig.url}`)
            }

            if (!await fs.exists(pluginFolder)){
                __log.error(`Plugin "${pluginName}" does not exist`)
                errors = true
                packageHasErrors = true
            }


            // ensure plugin has package manifest
            const packageManifestPath = path.join(pluginFolder, 'package.json')
            if (!packageHasErrors && !await fs.exists(packageManifestPath)){
                __log.error(`Plugin "${pluginName}" does not not have a package.json file`)
                errors = true
                packageHasErrors = true
            }

            // npm install plugin if it hasn't yet been installed, or if plugin's package.json has changed
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

        if (errors){
            __log.error('Plugin setup failed, shutting down')
            process.exit(1)
        }
    },


    /**
     * Ensures that all plugins are installed and up-to-date (git clone + npm install on all plugins). For a plugin to be 
     * installed it must be flagged as active
     */
    async initializeAll(){
        // this is where plugins will normally be installed
        let externalPluginsFolder = './server/plugins',
            // read the regular plugins list
            pluginsConfigPath = './plugins.json',
            testAll = !!settings.checkPluginsOnStart

        if (!await fs.exists(pluginsConfigPath)){
            __log.error(`ERROR - Expected plugins definition file ${path.resolve(pluginsConfigPath)} was not found. Please create this file and restart.`)
            return process.exit(1)
        }

        let pluginsConfig = await fs.readJson(pluginsConfigPath)

        // if a dev plugin list exists, load and merge that with the regular plugins list, let dev plugins override regular ones
        if (await fs.pathExists('./plugins.local.json')){
            const devPluginsConfig = await fs.readJson('./plugins.local.json')
            pluginsConfig = Object.assign(pluginsConfig, devPluginsConfig)
        }


        // set plugin source to internal if no source defined
        for (const plugin in pluginsConfig)
            pluginsConfig[plugin].source = pluginsConfig[plugin].source || 'internal'


        // we always need an auth / users plugin, add fallback if none defined
        pluginsConfig['wbtb-internalusers'] = pluginsConfig['wbtb-internalusers']|| { source : 'internal' }
        __log.info(`Added fallback authProvider plugin wbtb-internalusers`)


        // strip out all plugin config if explicitly disabled 
        for (const pluginName in pluginsConfig)
            if (pluginsConfig[pluginName].enabled === false){
                delete pluginsConfig[pluginName]
                __log.info(`Plugin "${pluginName}" is marked as disabled`)
            }


        // validate plugin.json static config
        let errors = false
        if (testAll)
            await this._ensurePluginsValid(pluginsConfig)

        await fs.ensureDir(externalPluginsFolder)

        // copy internal plugins
        if (testAll)
            await this._copyInternalPlugins(pluginsConfig)

        // install all external plugins, npm install on all plugins
        if (testAll)
            await this._setupAllPlugins(pluginsConfig)


        const installedPluginFolders = await this.getPluginsFolders()

        // bind all discovered plugins
        for (let pluginFolderPath of installedPluginFolders){
            const pluginName = path.basename(pluginFolderPath),
                pluginPackageJsonPath = path.join(pluginFolderPath,  'package.json'),
                pluginConfig = pluginsConfig[pluginName]

            // plugin folder exists locally, but is not defined in pluginsConfig, ignore it
            if (!pluginConfig)
                continue

            // validate package.json structure
            const packageJson = await fs.readJson(pluginPackageJsonPath)
            if (!packageJson.wbtb){
                __log.error(`ERROR : Plugin "${pluginName}" missing package.json:wbtb`)
                errors = true
                continue
            }

            // validate package.json wbtb config - plugin category
            if (!packageJson.wbtb.category){
                __log.error(`ERROR : Plugin "${pluginName}" missing package.json:wbtb.category`)
                errors = true
                continue
            }

            // merge local plugin config with .wbtb member of package.json - in this way, local config is available
            // via .wbtb to code
            packageJson.wbtb = Object.assign(pluginsConfig[pluginName], packageJson.wbtb)

            _plugins.byCategory[packageJson.wbtb.category] = _plugins.byCategory[packageJson.wbtb.category] || {}
            
            // default structure for plugin definition data is taken straight package.json as the "wbtb" section. This means
            _plugins.byCategory[packageJson.wbtb.category][pluginName] = packageJson.wbtb 
            
            // add other stuff to definition - path f.ex is calculated at load time.
            _plugins.byCategory[packageJson.wbtb.category][pluginName].path = pluginFolderPath 

            // wbtb uses package's name as its "id" value, id is a code used for data linking, name is used to display to humans
            _plugins.byCategory[packageJson.wbtb.category][pluginName].id = packageJson.name

            // ensure name, fall back to package name if none is set
            _plugins.byCategory[packageJson.wbtb.category][pluginName].name = _plugins.byCategory[packageJson.wbtb.category][pluginName].name || packageJson.name 

            // keep a copy of the plugin definition in a second hash table by plugin name, this if lookup if we don't know a plugin's category
            _plugins.plugins[pluginName] = _plugins.byCategory[packageJson.wbtb.category][pluginName]

            __log.info(`Plugin "${pluginName}" loaded`)
        }


        // fail if required plugins missing
        for (const requiredCategory of requiredCategories)
            if (!_plugins.byCategory[requiredCategory]){
                errors = true
                __log.error(`Required plugin category "${requiredCategory}" not found`)
            }


        // fail if a plugin category that can exist only once is overbooked
        for (const exclusiveCategory of exclusiveCategories){
            if (!_plugins.byCategory[exclusiveCategory])
                continue
            
            const keys = Object.keys(_plugins.byCategory[exclusiveCategory])
            if (keys.length > 1){
                errors = true
                __log.error(`Only 1 plugin of category "${exclusiveCategory}" is allowed`)
            }
        }
        
        if (errors){
            __log.error(`One or more plugins have invalid internal configuration - Who Broke The Build cannot start`)
            return process.exit(1)
        }

        const allPlugins = this.getAll()

        // generate UI route index file for all plugins which expose their own UIs
        await this._generateRouteIndexData(allPlugins)

        // initialize plugin
        await this._initializeAllPlugins(allPlugins)

        await fs.outputJson(pluginConfPath, _pluginConf, { spaces : 4})
    },


    /**
     * 
     */
    getPluginConf(){
        return _pluginConf
    },


    /**
     * Gets a single plugin by category, if more than one is registered for that category throws error
     */
    getExclusive(category){
        if (! _plugins.byCategory[category])
            throw new Exception({ code : constants.ERROR_MISSINGPLUGIN, message : `No plugin for category : ${category}, getExclusive() failed.` })

        const plugins = Object.keys( _plugins.byCategory[category])
        if (plugins.length > 1)
            throw new Exception({ code : constants.ERROR_MISSINGPLUGIN, message : `Multiple plugins found for category : ${category}, getExclusive() failed.` })

        const plugin = require(`${path.resolve(_plugins.byCategory[category][plugins[0]].path)}/index`)
        plugin.__wbtb = _plugins.byCategory[category][plugins[0]]
        return plugin
    },


    /**
     * 
    */
    get(name){
        const pluginPath = _plugins.plugins[name] ? _plugins.plugins[name].path : null
        if (!pluginPath)
            throw new Exception({ code : constants.ERROR_MISSINGPLUGIN, message : `Mising plugin : ${name}` })

        const plugin = require(`${path.resolve(pluginPath)}/index`)
        plugin.__wbtb = _plugins.plugins[name]
        return plugin
    },


    getAllByCategory(category, failOnNoMathes = true){
        const pluginsForCategory = _plugins.byCategory[category]
        if (!pluginsForCategory && failOnNoMathes)
            throw new Exception({ code : constants.ERROR_MISSINGPLUGIN, message : `Category : ${category}` })

        const plugins = []
        for (let pluginName in pluginsForCategory){
            const plugin = require(`${path.resolve(pluginsForCategory[pluginName].path)}/index`)
            plugin.__wbtb = pluginsForCategory[pluginName]
            plugins.push(plugin)
        }
        
        return plugins
    },


    /**
     * gets all active plugins
     */
    getAll(){
        let plugins = []
        for (let pluginPath in  _plugins.plugins){
            const plugin = require(`${path.resolve(_plugins.plugins[pluginPath].path)}/index`)
            plugin.__wbtb = _plugins.plugins[pluginPath]
            plugins.push(plugin)
        }
        
        return plugins
    },


    /**
     * Gets an array of typecodes from all plugins belonging to a given category
     */
    async getTypeCodesOf(category){
        const types = [],
            plugins = await this.getAllByCategory(category, false) // don't fail if no plugins of given type found

        for (let plugin of plugins)
            types.push(plugin.__wbtb.id) 

        return types
    },

    async validateSettings(){
        const plugins = await this.getAll()

        for(const plugin of plugins){
            if (!await plugin.validateSettings())
                throw `Settings validation failed for plugin ${plugin.__wbtb.id}`

            // check structure of plugins
            const requiredConfig = _pluginStructure[plugin.__wbtb.category]
            if (!requiredConfig){
                __log.warn(`Category ${plugin.__wbtb.category} has no interface structure enforcement. Consider adding this.`)
                continue
            }

            for (const member of requiredConfig)
                if (!plugin[member])
                    throw `Plugin ${plugin.__wbtb.id} does not impliment member "${member}" expected for category "${plugin.__wbtb.category}"`
        }
    },

    async runDiagnostic(){
        
    }
}   