# Who broke the build?

## Start

- Create a json file and `plugins.json`. Add some plugins to it. The following internal plugins are available f.ex

        {
            "wbtb-mongo" : { "source" : "internal" },
            "wbtb-internalusers" : { "source" : "internal" },
            "wbtb-activedirectory" : { "source" : "internal" },
            "wbtb-jenkins" : { "source" : "internal" }
        }

- Create a folder `plugins`, Chown 1000 it.
- start container


## Setup

To initialize mongo and mongo admin,
    
    cd /dev
    sh ./setup.sh
    docker-compose up -d
    
This is needed once only

The mongo admin interface is @

    http://localhost:3002

If not available, make sure the mongo db and admin containers have started properly.

You will probably want to run with dev plugins and sandboxing all services - create a .env file in your project root and add the following to it

    sandboxMode=true
    enableDevPlugins=true

`Sandboxmode` enables sandboxmode inside plugins - plugins which support this mode will mock calls to external servers using internal data. This is useful for developing without requiring actual running endpoint integrations. 

`enableDevPlugins` will load plugins from the `/server/plugins-internal` folder, which makes debugging and stepping through code possible.

## To run with live-reload and debug

Standard debug

    npm run dev

Standard debug without rebuilding CSS/JS on restart

    npm run dev -- --fast

Debug break

    npm run dev -- --brk

## To debug tests

node --inspect-brk=0.0.0.0:3001 tests/run-all.js ./tests

node --inspect-brk=0.0.0.0:3001 tests/run-all.js tests/server/logic/user

## Concepts

- everything is a plugin, all plugins can self-diagnose
- all plugins expose these three methods
    - verifySetup 
        runs a system diagnostic - can this plugin run on the current sytem, without specific config
    - verifyConfig
        does the plugin work with the config it's provided, f.ex, if it's a database plugin, can it connect to said database
    - isActive
        is the plugin enabled by the user. Normally verifySetup must always pass, but we run verifyConfig only if plugis is active

- all config is persisted in files, never in a database.
- secrets are 2-way encrypted using a persistent key

## Plugins

- plugins are standard nodejs modules, and rely on their own package.json to define behaviour
- plugins are pulled directly from npm or directly from git
- plugins are normally installed in the _/server/plugins_ folder. When developing a plugin it helps to have its source code stored along with the core application's code - in this case, plugins can also be placed in the _/server/plugins-dev_ folder.
- plugins for a given instance are defined in _/plugins.json_, which must be in the project root with the following structure

        {
            "pluginName": {
                "source" : "git",
                "url" : "https://github.com/shukriadams/node-fsUtils.git",
                "package" : "name"
                "version" : "0.0.1",
                "enabled" : true
            }
        }

    - source : git|npm
    - url : must be defined if source === git. This is git url. Must be accessible to this app. Use ssh or https token for private repo.
    - package : must be defined if source === npm. Package name on npm.
    - version : required
    - enabled : true|false. Optional, if false, config for this plugin will be ignored competely.

- add the wbtb json stub to package.json to describe it for wbtb's plugin manager

        {   
            ...

            "wbtb" : {
                "url" : giturl,
                "enabled: true,
                "config: custom config,
                "internal" : false // set to true for internal dev plugins
            }

            ...
        }

- A plugin _must_ have an index.js file in its root, which module.exports an object with the following base structure

        {
            // validates that a given plugin has all the settings it requires to function properly. Must return if passed
            // else plugin will be treated as non-functioning
            validateSettings()

            // returns string global name of plugin, this must be the same as the name defined in the plugin's package.json
            getDescription(){
                return {
                    id : string, global unique name of plugin, must be the same as defined in plugins's package.json
                    name : string, user-friednly name of plugin. will be used for 
                    uiroute : string of ui route. if not defined, none will be used. route will always be appended to id 
                }
            }
        }

- Plugin categories : ciserver|dataProvider

### Plugin Express

- a plugin can contain it's own Express UI. 
- Define all Express routes in a _routes.js_ file in root of the plugin folder. 
- All routes should be namespaced to that plugin, but beyond that a plugin is free to define its own structure. 
- All views must inhertic from the base layout view

        {{#extend "layout"}}
            {{#content "body"}}

                -- your view content ehre            

            {{/content}}
        {{/extend}}



