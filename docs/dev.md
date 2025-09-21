# Dev

## Requirements

- Dotnet 6.0
- NodeJS 12 or better
- A Postgres server, 4 or higher.
- sh at the command line - this is standard on Linux, and can be easily installed on Windows by installing git-for-windows and including its built-in sh.exe on your PATH.

## Get started

Set up frontend components (generally required once only when setting up project)

    cd ./src/Wbtb.Core.Web/frontend
    sh ./setup.sh
    npm install
    npm run icons
    npm run build

If you update frontend code after this you can rebuild with

    cd ./src/Wbtb.Core.Web/frontend
    npm run build

Install in-place C# libs with
    
    cd ./src
    sh ./setup-dependencies.sh

WBTB needs some basic config to work. There is a local developer config in the project at `/src/demo.config.yml`, which will create a project with example builds. Create a file `/src/.env`, and add the following to it:

    PG_HOST=<POSTGRES SERVER>
    PG_DATABASE=<POSTGRES DATABASE>
    PG_USER=<POSTGRES USER>
    PG_ADMIN_PWD=<POSTGRES PASSWORD>
    WBTB_CONFIG_PATH=<ABSOLUTE-PATH-TO-DEMO.CONFIG.YML>
    WBTB_HOSTNAME=http://myserver.local
    WBTB_MAX_THREADS=1
    WBTB_DAEMON_INTERVAL=1
    WBTB_FORCE_PLUGIN_VALIDATION=true
    WBTB_PROXYMODE=direct
    WBTB_SLACK_CHANNEL=123
    WBTB_LOG_STATUS_VERBOSITY=4
    WBTB_LOG_DEBUG_VERBOSITY=4

Replace <ABSOLUTE-PATH-TO-DEMO.CONFIG.YML> with the absolute path to this file in your checkout, and all values in <POSTGRES..> with your Postgres info. Leave everything else in place. You're now ready to go.

If you're running Visual Studio, you can start the project immediately. Open `\src\Wbtb.sln`, set `Wbtb.Core.Web` as the startup project, and lauch the `WbtbWeb` debug profile.

If you're running from the command line, build WBTB

    cd ./src
    dotnet restore Wbtb.Core.Web
    dotnet build Wbtb.Core.Web

To start

    cd ./src
    dotnet run --project Wbtb.Core.Web --urls=http://0.0.0.0:5000

Open `localhost:5000` in your browser.

## Config

Wbtb requires a lot of configuration to work. Config is provided in two ways - env variables, and a single config.yml file. There is some overlap between them. Generally, env variables are used for basic settings that Wbtb needs to start, and config.yml for complex settings Wbtb needs to do work.

## Env variables

All standard ways to set env vars in Dotnet and Visual Studio are valid. Wbtb provides an additional mechanism, where it will automtically look for a .env file in /src/Wbtb.Core.Web` or any parent directory of that, all the way up to the disk root. This file should contain one my_var=some_value pair per line. The file is already in .gitignore, and is a convenient place for dev-time values like connection strings etc.

## Config.yml

Config.yml contains a YML representation of Wbtb's Configuration class. It normally lives in the application execution root directory, `./src/Wbtb.Core.Web/bin/Debug/<CURRENT DOTNET RUNTIME>/` in VStudio. You can override where Wbtb looks for this file by setting the environment variable `WBTB_CONFIG_PATH`. 

Wbtb also supports getting config.yml from a git repo. You can add a git url string in a file at `./src/Wbtb.Core.Web/.giturl`, or set the git url with the `WBTB_GIT_CONFIG_REPO_URL` env var. The URL must be self-authenticating, ie, contain its own auth credentials, Wbtb does not currently support SSH authentication. Wbtb will attempt to check the repo out to './src/Wbtb.Core.Web/bin/Debug/net6.0/<your-data-root>/ConfigCheckout

## Secrets

Wbtb has rudimentary support for secrets using environment variables and config templating. To avoid storing secrets in config.yml, you can add the template string "{{env.MY_VALUE}}" (without quotes) anywhere in your config file. If you also define an environment variable 'MY_VALUE', its value will be automatically used when reading the contents of config.yml.

## Coding and Debugging

Debugging Wbtb is admittedly not always a simple matter of loading the solution in Visual Studio and hitting F5.

### Simple setup

WBTB can be configured to run all plugins in a single application context, allowing you to step into all C# code when running the core server. This is intended for use during plugin development.

Add your plugin to the WBTB solution, and then as Project dependency to Wbtb.Core.Web. You don't need to register it as a dependency, the plugin manager will do this automatically assuming the plugin has a valid Wbtb.yml file in its root.

### Direct messaging setup

When running in production, plugins communicate via a combination of shell and HTTP. HTTP traffic is handled by the MessageQueue app. For convenience this isn't necessary in dev environments, and can be disabled by setting the environment variable `WBTB_PROXYMODE` to `direct`.

### Diagnostic mode

You can start any C# plugin in diagnostic mode by starting it with the `--diagnostic` switch. This mode is primarily intended to ensure that the plugin application can start properly and enters a state where incoming message can be received. The plugin will automatically invoke its `Diagnose` method, which you can override locally, then exit immediately.

Diagnostic mode requires a running Messagequeue instance for the plugin to connect to. The easiest way to achieve this is to manually start the MessageQueue service, then open the solution in another instance of Visual Studio, set `ForceMessageQueue=true` in config.yml, and start the web server. This will prime the MessageQueue with a valid config.

### Disconnected debugging

WBTB plugins run as independent scripts or CLI applications, making debugging in a single application context impossible. There are mitigations however. 

You can run multiple instances of Visual Studio on the same application, but with different starting apps. Set one instance to start the core app, and the second to run your plugin. Plugins are executables that run stateless single operations. Start your plugin with the arguments passed to it by the core, and it will perform a single operation - you can apply breakpoints etc.

Before debugging be sure to run the messagequeue service with `--persist` to ensure that invocation data passed between various sub-systems in WBTB doesn't get timeout or get removed.

## Project Config

Wbtb requires basic configuration to function - starting it without this will cause the solution to exit immediately with a config error.

Project config must be serializable, as it passed out to plugin executables as JSON. It should therefore not contain types that cannot be easily serialzied, such as Type.

## Why not fluent nhibernate or similar

WBTB's core feature is a distrubuted, cross-runtime plugin system. Data objects from frameworks like Nhibernatecannot cross between objects while maintaining their live-updating ability, which is one of the core reasons for using them. 

## Plugins

- Plugins should always be stateless. A plugin is instantiated to invoke a function on it, once the function exists, the plugin instance exits and all state is lost. If you want to persist values in a custom plugin across calls, you'll need to store it some place outside of the plugin, like on disk.

- In theory, you can implement a plugin in any way you want, as long as it respects WBTB's HTTP and CLI interface requirements (TBD), and has a WBTB manifest yml file in its root directory. If you're working in C# and want to use the WBTB.Common library as a base to get a plugin working quickly, your plugin should implement one of the standard WBTB plugin interfaces, and should have either a parameterless-constructor, or constructor arguments which are registered with WBTB's dependency injection system. You can register your own types in your plugin if you want.

## Dependencies

Common has as few dependencies as possible to reduce potential for dependency version conflicts within plugins.

## Worker order

Wbtb handles build logic with a series of daemon workers. Each daemon is a process that runs on its own thread and processes a "stage" in the lifecycle of a build. A given daemon will process a build by reading the database for DaemonTask records. Tasks are completed in order, and can be used to queue and track work.
---------------------------------------------------------
0
- build create daemon > runs unrestricted, not tied to any daemontasks. polls build server for new builds, writes those builds to db. creates. OUT : BuildEnd.

- build end daemon > IN:BuildEnd. This daemon marks build as complete. Ensures that builds are forced abandoned if they time out from server. Creates revisopn resolve tasks if readrevfromlog not set on job. Creates buildinvolvements. OUT : IncidentAssign, LogImport.
---------------------------------------------------------
1
- assign incident to build. IN: IncidentAssign.

- log import > imports log from build server. waits for build to be marked complete. writes logprocesstask for each logprocessor on job . OUT. LogParse, ReadRevisionFromLog

---------------------------------------------------------
2 
- read revision in log > Task:ReadRevisionFromLog. Reads revision in log once log imported, create buildinvolvements for that revision + preceeding one. Waits for log to be imported. Used only on jobs with readrevfromlog enabled. Creates buildinvolvements. Creates revisionresolve tasks. 

- resolve buildinvolvement revisions from source control > processes revision resolve tasks. Creates resolveuser tasks. modifies buildinvolvement.
---------------------------------------------------------
3
- parse build log. IN : LogParse
waits for logprocesstask, logparsers can be run in parallel

- resolve revisions from source control

- resolve user on buildinvolvement. Waits for revision resolve. modifies buildinvolvement.
---------------------------------------------------------
4
- blame daemon. waits for log to be parsed, incident to assigned, revision to be resolved. modifies buildinvolvement.

- calculate current delta. waits for all incident on a job to be completed, delta set to latest incident on job

- alert on delta. waits for all tasks for a job to be done, then alerts if delta different from last reported delta. 

---------------------------------------------------------

Because build data is interrelated and results from one build can affect subsequent builds, builds are processed in series, one at a time, oldest to newest, and an error in the processing of one build will block all processing. 

## Parsers

Parsers are plugins which parse logs. Normally a given plugin parses a specific thing or category of thing. Multiple parsers run on a single build log, each getting a chance to find the thing they're looking for. Parsers return the strings they look for, or nothing. 

WBTB parsers can return raw extracted log text, exactly as it appears in the source log, or they can wrap their text in a simplified form of markup. This markup is displayed on Wbtb log web page, but can also be used by post-processor plugings to collate all log extractions and try to determine the most likely cause of build failures.

## Post processors

Post processors are amongst the last plugins run on a build event. They can be used to determine causes of build failures, using log extractions generated by parsers. Typically, several post processors are run on a build.