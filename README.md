# Who broke the build?

*Who Broke the Build* helps regular people interact with build data. It gathers build results, logs etc, and condenses them down into a format that is easy to read and follow. Complex build and infrastructure data is flattened into a few simple lists. Build logs are parsed so only relevant errors are displayed. Source code changes and the people who wrote those changes are parsed out.

Additionally, *Who Broke The Build* lets you directly message people on your team if they broke the build, using tools like Slack. It sends simple and meaningful messages. It also alerts other people like team leads, and help you triage and communicate around specific build breaks.

*Who Broke The Build* is written in Javascript in NodeJS. It uses a plugin system, and comes with integrates for common version control systems like Git, Perforce and SVN, as well as build servers like Jenkins and TeamCity. You can write your own plugins and integrate them easily using NPM, or Git.

## Start

- set your current dir to the src folder of this project

        cd /src

- Create a `settings.yml` file. Add a `plugins` node to it, then define the plugins you want to use - the following internal plugins are available f.ex

        plugins:
            wbtb-activedirectory:
            wbtb-internalusers:
            wbtb-mongo:
            wbtb-jenkins:

    A plugin has the following standard options

        plugins:
            plugin-name:
                enabled: trues|false
                source: 'internal'|git:<git url>|npm:<npm name>

    `plugin-name` must exactly match the name of the plugin in that plugin's `package.json` file. 

- Start app with `npm start` or `node index`

## OnStart Logic

If you need WBTB to set up your host system, place a script called `onStart.sh` in the project root, this will be executed when WBTB starts. This is particularly useful when running in Docker and you need to ensure that host state has been set. We use it to force Perforce trust for example.

Note that errors from this script will caused WBTB to exit, but apps like P4 are known for "chatter" on stderr. To get around this you can squelch errors in onStart.sh by routing output to dev null

    <your command> 2>/dev/null


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

## Config

WBTB requires config to start, this must be created locally or passed in if running in a container. Typical config looks like

    bindInternalPlugins: true
    logLevel: debug
    plugins:
        wbtb-activedirectory:
            url: ldap://your-server:IP
            base: "dc=yourdomain,dc=local"
            user: username
            password: password
        wbtb-busybody:
        wbtb-internalusers:
        wbtb-mongo:
            connectionString: "mongodb://username:password@mongo.server.com:27017"
            db: wbtb
        wbtb-jenkins:
            cacheAPICalls: true
        wbtb-svn:
        wbtb-p4jenkinsmockdata:
        wbtb-messagebuilder:
        wbtb-perforce:
            maxCommitSize: 5000
        wbtb-unreallogparser:
        acmegameco-standalonerevisionlinker:

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
            // validates that a given plugin has all the settings it requires to function properly. Must return true if passed
            // else plugin will be treated as non-functioning
            validateSettings()
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



