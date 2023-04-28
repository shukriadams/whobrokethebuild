# Who broke the build?

A server to process build data from CI servers like Jenkins, aiding communication with large development teams, particularly those in game development. WBTB is written primarily in C# on the .Net Core runtime, is Docker-ready and is developed Linux-first.

## Basic Setup

### Database

WBTB requires a database to function. Currently PostgreSQL is supported. It will automatically create 

### Basic config

All WBTB's configuration is kept in a single YML file in the application root directory. WBTB attempts to load and validate this file on application start, and will exit if your configuration is valid. 

To set up a basic demonstration server, create or mount a file called `config.yml` in the app root directory (typically the directory with Wbtb.Core.Web.dll) and set its content to

    Plugins: 
    -   Key: Postgres
        Path: Wbtb.Extensions.Data.Postgres
        Config:
        - Host: <postgres-address>
        - User: <postgres-user>
        - Password: <postgres-password>
        - Database: <postgres-database>
    -   Key: jenkinsSandbox
        Path : Wbtb.Extensions.Data.JenkinsSandbox
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

Assuming you're running WBTB from a Docker container image, start your container and point it to the above config. All required data tables will be automatically created in the Postgres database pointed to, build data will be pulled from from sandbox sources for Jenkins and Perforce, and saved to the database. For additional information on working with the built-in dummy datasources in WBTB, or running a WBTB from source code, check the the /docs directory of this project.
