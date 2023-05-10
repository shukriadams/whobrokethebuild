# Dev

## Requirements

- Dotnet 6.0
- NodeJS 12 or better

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

and to start the server

    cd src
    dotnet run --project Wbtb.Core.Web --urls=http://0.0.0.0:5000

The url above may be required in Vagrant, depending on your VM provider platform.

## Developer settings

Normally Wbtb settings are written to a config.yml file in the application execution root. In visual studio you can specify additional config in two ways.

You can add a `.env` file to the root of the Visual Studio project you're running from. This file is already listed in the included .gitignore, so it's a good place to put secrets etc that you don't want commited, or overwritten. The file uses a name=value syntax.

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

## Build processing daemon order

1) Build records are created or updated by the BuildImport daemon. This daemon sets status to pass/fail as part of update, and also sets delta on builds. If build revision can be read from build server, this is used to set up buildInvolvement records.

2) Build logs are imported some tom after build completion by the BuildLogImport daemon.

3) Build servers don't always expose explicit information about which revisions are ina  build. The BuildRevisionFromLogDaemon calculates revision by polling logs from ongoing builds until build info can be read. This sets up buildInvolvement records.

4) If a build fails, it can be assigned an incident code, which groups it with other failing builds. This is done by the IncidentAssignDaemon, and is done only after a build's status is set to failing, which is done by BuildImport's update cycle.

5) Builds logs can be parsed, this done by the LogParser daemon, and only on builds that have already had their logs downloaded by the BuildLogImport daemon.

6) BuildInvolvements can be linked to detailed revision data, this is done by the RevisionResolveDaemon, once a BuildInvolvement record is created.

7) Buildinvolvements can be linked to detailed user data, this is done by the UserBuildInvolvementLinkDaemon, once a BuildInvolvement record is created.

Based on the above sequence, it's quite clear that build state cannot be implicitly inferred on-the-fly, as the existence of a given record doesn't necessarily mean the record has been fully processed yet. Generally, users of the system will approach it with specific intent :

- Is a job broken?
- Did I break the job?
- Is my revision in a commit that broken the job?
- Did my revision fix a broken job?
- What code change broken the job? (using logs)
- Which revisions broke the job?

Beyond that, developers will want to hook up to certain events, with the expectation that certain state is available to them. 