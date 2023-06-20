# Dev

## Requirements

- Dotnet 6.0
- NodeJS 12 or better
- sh at the command line - this is standard on Linux, and can be easily installed on Windows by installing git-for-windows and including its built-in sh.exe on your PATH.

## Setup

The suggested way to work on this project is in Vagrant. A complete dev environment is defined in source code already. After cloning, from the project root

    cd vagrant/<your-vm-flavor>
    vagrant up
    vagrant ssh

then once inside the wbtb guest vm
 
to set up frontend components (generally required once only when setting up project)

    cd src/Wbtb.Core.Web/frontend
    sh ./setup.sh
    npm install
    npm run icons
    npm run build

if you update frontend code after this 

    cd src/Wbtb.Core.Web/frontend
    npm run build

to build the server

    cd src
    dotnet restore Wbtb.Core.Web
    dotnet build Wbtb.Core.Web

In most cases, Wbtb will require a separate service, MessageQueue, to be started as a process. You

    cd src
    dotnet restore MessageQueue
    dotnet build MessageQueue
    dotnet run --project MessageQueue

To start the Wbtb server

    cd src
    dotnet run --project Wbtb.Core.Web --urls=http://0.0.0.0:5000

The url above may be required in Vagrant, depending on your VM provider platform.

## Developer settings

Normally Wbtb settings are written to a config.yml file in the application execution root. In visual studio you can specify additional config in two ways.

You can add a `.env` file to the root of the Visual Studio project you're running from (most likely Wbtb.Core.Web/). This file is already listed in the included .gitignore, so it's a good place to put secrets etc that you don't want commited, or overwritten. The file uses a name=value syntax.

You can also add a `.giturl` file to the same location, for the repo you keep Wbtb's settings in. This is done because some url's contain `=` characters, which break the `.env` file.

## Coding and Debugging

Debugging WBTB is admittedly not always a simple matter of loading the solution in Visual Studio and hitting F5.

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

WBTB's core feature is a distrubuted, cross-runtime plugin system. Data objects from frameworks likeNhibernatecannot cross between objects while maintaining their live-updating ability, which is one of the core reasons for using them. 

## Plugins

- Plugins should always be stateless. A plugin is instantiated to invoke a function on it, once the function exists, the plugin instance is assumed destroyed.

- In theory, you can implement a plugin in way you want, as long as it respects WBTB's HTTP and CLI interface requirements (TBD), and has a WBTB manifest yml file in its root directory. If you're working in C# and want to use the WBTB.Common library as a base to get a plugin working quickly, your plugin should implement one of the standard WBTB plugin interfaces, and should have either a parameterless-constructor, or constructor arguments which are registered with WBTB's dependency injection system. You can register your own types in your plugin if you want.

## Dependencies

Common must have as few dependencies as possible to reduce potential for dependency version conflicts within plugins.

## IOC

No ioc in plugins, don't force or make assumptions about how plugins will be written.

plugins get passed an instance of config which they must keep alive themselves

## Daemon order

Wbtb handles build logic with a series of daemons. Each daemon is a process that runs on its own thread and processes a "stage" in the lifecycle of a build. A given daemon will process a given build or a child record of a build by reading the database for DaemonTask records. Tasks are completed in order, and can be used to queue and track work.

- build update daemon > Task:BuildComplete. This daemon marks build as complete. Ensures that builds are forced abandoned if they time out from server. Creates revisopn resolve tasks if readrevfromlog not set on job. Creates buildinvolvements.
- assign incident to build. Waits for build update.
- log import > imports log from build server. waits for build to be marked complete. writes logprocesstask for each logprocessor on job
- read revision in log > Task:ReadRevisionFromLog. Reads revision in log once log imported, create buildinvolvements for that revision + preceeding one. Waits for log to be imported. Used only on jobs with readrevfromlog enabled. Creates buildinvolvements. Creates revisionresolve tasks. 
- resolve buildinvolvement revisions from source control > processes revision resolve tasks. Creates resolveuser tasks. modifies buildinvolvement.
- resolve user on buildinvolvement. Waits for revision resolve. modifies buildinvolvement.
- parse build log. waits for logprocesstask, logparsers can be run in parallel
- blame daemon. waits for log to be parsed, incident to assigned, revision to be resolved. modifies buildinvolvement.
- calculate current delta. waits for all incident on a job to be completed, delta set to latest incident on job
- alert on delta. waits for all tasks for a job to be done, then alerts if delta different from last reported delta. 
