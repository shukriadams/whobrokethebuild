# Dev

## Requirements

- Dotnet 6.0
- NodeJS 12 or better
- A Postgres server, 4 or higher.
- sh at the command line - this is standard on Linux, and can be easily installed on Windows by installing git-for-windows and including its built-in sh.exe on your PATH.

## Setup

### Vagrant

You can set up a complete dev environment using Vagrant. After cloning, from the project root

    cd ./vagrant/<your-hypervisor-flavor>
    vagrant up
    vagrant ssh

Once inside the wbtb guest vm, set up frontend components (generally required once only when setting up project)

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

To build the server 

    cd ./src
    dotnet restore Wbtb.Core.Web
    dotnet build Wbtb.Core.Web

In most cases, Wbtb will require a separate service, MessageQueue, to be started as a process running in the background. 

    cd ./src
    dotnet restore MessageQueue
    dotnet build MessageQueue
    dotnet run --project MessageQueue

To start the Wbtb server

    cd ./src
    dotnet run --project Wbtb.Core.Web --urls=http://0.0.0.0:5000

The url argument above may be required in Vagrant.

## Developer config and settings

TL;DR: WBTB needs some basic config to work. There is a local developer config in the project, /docs/config.yml, ready to go. There are several ways to to use it, the simplest is :

1. edit the file `src/Wbtb.Core.Web/Properties/launchSettings.json`. To its `environmentVariables` section add `"WBTB_CONFIG_PATH": "<PATH>"` where <PATH> is the absolute path to where that config.yml file is checked out on your system.

2. Create a file `src/Wbtb.Core.Web/.env`, and add the following to it:

    WBTB_HOSTNAME=http://myserver.local
    WBTB_MAX_THREADS=1
    WBTB_DAEMON_INTERVAL=1
    PG_HOST=<POSTGRES SERVER>
    PG_DATABASE=<POSTGRES DATABASE>
    PG_USER=<POSTGRES USER>
    PG_ADMIN_PWD=<POSTGRES PASSWORD>
    WBTB_SLACK_CHANNEL=123

Replace all values in <..> with your Postgres info. You're now ready to go.

The longer version ....

Wbtb requires a lot of configuration to work, and has several ways to manage this. Its most basic settings can be passed in as environment variables, while others should be defined in a config.yml file. By default, config.yml is expected in the application execution root directory './src/Wbtb.Core.Web/bin/Debug/net6.0/', but you can override this path with the environment variable `WBTB_CONFIG_PATH`. Visual Studio has built in ways of managing environment variables, these will all work. For convenience Wbtb has an additional mechanism - you can place all env vars in a file at the path `./src/Wbtb.Core.Web/.env`. This file should contain one my_var=some_value pair per line, and is already in .gitignore. This is a convenient place to place dev-time connection strings etc.

Wbtb also supports getting config.yml from a git repo. You can add a git url string in a file at `./src/Wbtb.Core.Web/.giturl`, or set the git url with the `WBTB_GIT_CONFIG_REPO_URL` env var. The URL must be self-authenticating, ie, contain its own auth credentials, Wbtb does not currently support SSH authentication. Wbtb will attempt to check the repo out to './src/Wbtb.Core.Web/bin/Debug/net6.0/<your-data-root>/ConfigCheckout

## Secrets

Wbtb has rudimentary support for secrets using environment variables and config templating. To avoid storing secrets in config.yml, you can add the template string "{{env.MY_VALUE}}" (without quotes) anywhere in your config file. If you also define an environment variable 'MY_VALUE', its value will be automatically used when reading the contents of config.yml.

## Coding and Debugging

Debugging Wbtb is admittedly not always a simple matter of loading the solution in Visual Studio and hitting F5.

### Simple setup

WBTB can be configured to run all plugins in a single application context, allowing you to step into all C# code when running the core server. This is intended for use during plugin development.

Add your plugin to the WBTB solution, and then as Project dependency to Wbtb.Core.Web. You don't need to register it as a dependency, the plugin manager will do this automatically assuming the plugin has a valid Wbtb.yml file in its root.

### Direct messaging setup

Normally plugins communicate via a combination of shell and HTTP. Shell interaction is the riskiest of the two, and is therefore locked down to very simple interactions that are kept stable. HTTP interaction are more suscepitble to variable conditions, and you can run a plugin in the same application context as the core app, but where all communication to plugins still happens over http. Set the environment variable `WBTB_PROXYMODE` to `direct` to enable this.

### Diagnostic mode

You can start any C# plugin in diagnostic mode by starting it with the `--diagnostic` switch. This mode is primarily intended to ensure that the plugin application can start properly and enters a state where incoming message can be received. The plugin will automatically invoke its `Diagnose` method, which you can override locally, then exit immediately.

Diagnostic mode requires a running Messagequeue instance for the plugin to connect to. The easiest way to achieve this is manually start a MessageQueue server, then open the solution in another instance of Visual Studio, set `ForceMessageQueue=true` in config.yml and start the server. This will prime the MessageQueue with a valid config.

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

- Plugins should always be stateless. A plugin is instantiated to invoke a function on it, once the function exists, the plugin instance is destroyed. If you want to persist values in a custom plugin across calls, you'll need to store it some place outside of the plugin, like on disk.

- In theory, you can implement a plugin in any way you want, as long as it respects WBTB's HTTP and CLI interface requirements (TBD), and has a WBTB manifest yml file in its root directory. If you're working in C# and want to use the WBTB.Common library as a base to get a plugin working quickly, your plugin should implement one of the standard WBTB plugin interfaces, and should have either a parameterless-constructor, or constructor arguments which are registered with WBTB's dependency injection system. You can register your own types in your plugin if you want.

## Dependencies

Common has as few dependencies as possible to reduce potential for dependency version conflicts within plugins.

## IOC

No ioc in plugins, don't force or make assumptions about how plugins will be written. Plugins get passed an instance of config which they must keep alive themselves

## Daemon order

Wbtb handles build logic with a series of daemons. Each daemon is a process that runs on its own thread and processes a "stage" in the lifecycle of a build. A given daemon will process a given build or a child record of a build by reading the database for DaemonTask records. Tasks are completed in order, and can be used to queue and track work.
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