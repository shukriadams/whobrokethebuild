# Who broke the build?

A server to process build data from CI servers like Jenkins, aiding communication with large development teams, particularly those in game development. WBTB is written in C#, Docker-ready and developed Linux-first. It supports Perforce and Unreal Engine out of the box, and all of its integrations are plugins.

## Basic Setup

### Database

WBTB requires a database. Currently only PostgreSQL is supported.

### Configuration 

All WBTB's configuration is kept in a single YML file. WBTB attempts to load and validate this file on application start, and will exit if your configuration is invalid, displaying a console message with how to fix the error.

#### Basic config

If you want to look around and inspect how WBTB works, you can spin up an instance quickly with Docker. 

To set up a basic demonstration server, create a file called `config.yml` and set its content to

    Plugins: 
    -   Key: Postgres
        Path: var/wbtb/Wbtb.Extensions.Data.Postgres
        Config:
        - Host: <postgres-address>
        - User: <postgres-user>
        - Password: <postgres-password>
        - Database: <postgres-database>
    -   Key: jenkinsSandbox
        Path : Wbtb.Extensions.BuildServer.JenkinsSandbox
    -   Key: p4Sandbox
        Path: Wbtb.Extensions.SourceServer.PerforceSandbox

    SourceServers:
    -   Key: myperforce
        Plugin: p4Sandbox

    BuildServers:
    -   Key: MyJenkins
        Plugin: jenkinsSandbox
        Jobs:
        -   Key: Project_Ironbird
            SourceServer: myperforce

Assuming you're running WBTB from the official Docker container image, volume mount this file in /wbtb/config.yml. Your database will be populated. `Project_Ironbird` is one of the demonstration projects included in the sandbox Jenkins plugin. For additional information on working with the built-in dummy datasources in WBTB, or running a WBTB instance from source code, check the /docs directory of this project.

#### Production config

In progress ....


