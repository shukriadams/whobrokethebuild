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
    dotnet run --project Wbtb.Core.Web

## Coding and Debugging

Debugging WBTB is admittedly not always a simple matter of loading the solution in Visual Studio and hitting F5.

## Simple setup

WBTB _can_ be configured to run all plugins in a single application context, allowing you to step into all C# code when running the core server. This is intended for use during plugin development.

## Direct messaging

Normally plugins communicate via a combination of shell and HTTP. Shell interaction is the riskiest of the two, and is therefore locked down to very simple interactions that are kept stable. HTTP interaction are more suscepitble to variable conditions, and you can run a plugin in the same application context as the core app, but where all communication to plugins still happens over http. Set the environment variable `WBTB_PROXYMODE` to `direct` to enable this.

## Disconnected debugging

WBTB plugins run as independent scripts or CLI applications, making debugging in a single application context impossible. There are mitigations however. You can run multiple instances of Visual Studio on the same application, but with different starting apps. Set one instance to start the core app, and the second to run your plugin. Plugins are executables that run stateless single operations. Start your plugin with the arguments passed to it by the core, and it will perform a single operation - you can apply breakpoints etc.

Before debugging be sure to run the messagequeue service with `--persist` to ensure that invocation data passed between various sub-systems in WBTB doesn't get timeout or get removed.

## Project Config

Wbtb requires basic configuration to function - starting it without this will cause the solution to exit immediately with a config error.

Project config must be serializable, as it passed out to plugin executables as JSON. It should therefore not contain types that cannot be easily serialzied, such as Type.

## Why not fluent nhibernate

- I didn't want to expose nhibernate objects to the entire app, it's convenient to have objects live linked to each other, but this will be negated by WBTB's disconnected plugin architecture.

- I found nhibernate too opinionated in how it wanted objects to be structured. It expects everything to follow 

## Plugins

- Plugins should always be stateless
- Do not not use any constructor logic in your plugin, as it will be initialized cross-domain by a system that cannot resolve constructor dependencies etc. 

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