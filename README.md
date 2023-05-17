# Who broke the build?

A server to process build data from CI servers like Jenkins, aiding communication with large development teams, particularly those in game development. WBTB is written primarily in C# on the .Net Core runtime, is Docker-ready and is developed Linux-first.

## Basic Setup

### Database

WBTB requires a database to function. Currently PostgreSQL is supported. It will automatically create 

### Configuration 

All WBTB's configuration is kept in a single YML file. WBTB attempts to load and validate this file on application start, and will exit if your configuration is invalid. 

#### Basic config

If you want to look around and inspect how WBTB works, you can spin up an instance quickly with Docker. 

To set up a basic demonstration server, create a file called `config.yml` and set its content to

    Plugins: 
    -   Key: Postgres
        Path: Wbtb.Extensions.Data.Postgres
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

Assuming you're running WBTB from the official Docker container image, volume mount this file in /wbtb/config.yml. All required data tables will be automatically created in the Postgres database pointed to, build and source data will be pulled from sandbox Jenkins and Perforce sources, and saved to the database. `Project_Ironbird` is one of the demonstration projects included in the sandbox Jenkins plugin. For additional information on working with the built-in dummy datasources in WBTB, or running a WBTB instance from source code, check the the /docs directory of this project.

#### Production config

in progress ....


