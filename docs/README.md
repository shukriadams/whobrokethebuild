# WBTB

Who Broke The Build? is a server/service for managing build data in a CI environment. It gathers information about builds, including build state, logs and source code information, for the purpose of communicating build state to a development team. Basically, it tells your team when a build is broken, what broke, and who broke it. It is aimed specifically at game developers working on large teams, large code bases and multiple build targets. 

WBTB has the following features :

- All integrations are done as plugins, and you can extend it by adding your own plugins.
- WBTB plugins can be written in any programming language that runs in a shell and supports HTTP. It already supports C#, Javascript and Python.
- WBTB config can be managed entirely from git.
- WBTB is written to take advantage of modern multi-core CPUs.
- WBTB requires a Postgres database to operate, but is not reliant on the database content - the database content can be deleted and WBTB will repopulate it.

These docs are a still a WIP.

## Todo

- database persistence
- config checking
- web views

## Setup

WBTB requires a data backend to function. Currently Postgres is supported. Add the following to your config.yml

    Plugins: 
    -   Key: myPostgres
        Proxy: false
        Path: C:\projects\wbtb\src\Wbtb.Extensions.Data.Postgres\bin\Debug\netcoreapp3.1
        Config:
        -   User: myuser
        -   Host: mypostgres.example.com
        -   Password: mypassword
        -   Database: mywbtb

## Local development

### Setup

Wbtb requires .Net 6.0. Building static frontend resources for Wbtb requires Nodejs 12 or higher. Nodejs isn't required to run the server in production mode. A full development environment setup is available in /vagrant/provision.sh of this repository.

### Run

- Clone project
- cd src/Wbtb.Core.Web/frontend
- sh ./setup.sh
- npm install
- npm run build
- start project in vs studio or build from cmnd line

## More on Config

- There is no admin interface. All config is written in a single yml file.
- When the app starts it will validate the yml, both for format, for logical structure, and finally, runtime tests to ensure that systems pointed to are available. 
- In the event of a configuration error the app will exit and give you some kind of error message explaining how to fix the config.
- Because WBTB is an open-ended system, plugins can define their own requirements for config. Config is limited to name:value items, and must always be placed under a `Config` node.
- Upper parts of config are type-safe and defined in the core app
- plugin-specific parts can be defined in a block called `Config` for each plugin. This is passed as a string to the plugin and can be deserialized there

    -   Id: my-perforce
        Interface: ISourceServerPlugin
        Proxy: false
        DirectBindType: Wbtb.Extensions.SourceServer.Perforce.Perforce
        Config:
        -   Host: p4.myserver:1666
        -   User: myuser
        -   Password: mypassword

    Users:
    -   Key: sad
        Source: AD
        SlackUserId: 1234
        Enable: false
        p4_1: c
        SourceServerIdentities:
        - Name : Bob
          SourceServerKey: my-p41

    BuildServers:
    -   Key: MyJenkins
        Name: Main Jenkins Server
        Host: jenkins.myserver.local

        Jobs:
        -   Key: myjob
            SourceServer: my-perforce
            # optional
            Name: myjob
            # optional, regex to parse revision out of build log
            RevisionAtBuildRegex: \#p4-changes.....\#\nChange (.*) on.*\n\#\/p4-changes......\#
            # optional
            RevisionScrapeSpanBuilds: true
            # optional
            ImportCount: 10
            # optional
            LogParserPlugins: 
            -   CPPLogParser
            # optional
            Image: /images/scrap.jpg

    SourceServers:
    -   Key: my-p41
        Plugin: WBTB-Perforce
        Enable: true
        Config:
        - Host: ssl:p4.myserver:1666
        - User: myuser
        - Password: mypass
        - Trust: true # perforce only

### Main concepts

There are several complex concepts in WBTB. 

1 - data objects can be defined in the config file. these objects are available at runtime. Complete with relationships.
2 - the same data objects defined in config file are also created in a database, but db records are always slaved to config file records. Restraints are enforced in the db. 
3 - Plugins are ways to connect to subsystems. in some cases plugins are closely connected with config objects, ie, build server and source control servers. There can be multiple buildservers connected to a WBTB instance, as well as multiple build server plugins. Some plugins must be associated with a config object to work.
5 - Keys are important. They normally reflect unique identities that persist in the world - employees, source control for projects that persist indefinitely, and build servers that perform thousands of builds and which have history. Pick keys that accurately describe these, are easy to understand, and can live alongside future keys. WBTB provides a mechanism to merge existing key-based data into new objects, but you probably won't be using this a lot.
6- WBTB auto-validates configuration on start, and validation is designed to give a clear indication of what went wrong and how to fix it. Iin most cases WBTB will not start until all validation errors are corrected.
7- WBTB can be run in staging mode, allowing you to test configuration changes outside your prodution environment before deploying them. 

#### Users

#### Groups

Groups allow users to be bound together, so operations that target multiple users can be aimed at a group. Groups are internal in WBTB, they cannot be mapped to external groups.

#### BuildServers

#### Jobs

Jobs are always defined under the build server they run on.

#### SourceServers

## Data and persistence

WBTB uses config-as-code - all of the required config to function is written in YML files. This config is used to create in-memory DTOs for WBTB to function. 

DTOs in turn can contain additional properties which aren't user configurable, and may therefore be persisted in a database. 

## Plugin

- Plugin must have metadata describing itself, this must be placed in a file called `wbtb.yml` in the plugin root folder

- plugin metadata must describe
    - runtime (dotnet|nodejs|python|python3)
    - executable target (main) file in plugin
    - plugin unique id
    - plugin version

- plugins are shell applications, and communciation with plugins is via stdin and http. Plugins can therefore be writting in any programming language that has http communication and can run at the CLI. Currently dotnet and Python are supported.

- restictions

- a plugin cannot expose two methods with the same name, ie, method overloading is not supported. This is to keep it inline with languages like Javascript that do not support overloading.

### Self-verifying config

Self-verification of config is a core concept of WBTB - config is flat, written in YML and stored in source control, so we need to ensure all of it is valid on app start, but where the config itself might be needed for the checks to work. To prevent tying ourselves up in a situation where our config checks end up relying on state which in turn is dependent on config checks, we need to set up rules.

1 - first we check the raw yml format is valid, as yml can be easily broken
2 - next we check that the data obejcts modelled in yml match what we expect, required fields are set etc.
3 - next we check yml object dependencies - many objects in WBTM can link to each other, linking is always done via static yml idenfitiers, so we check that the objects we point to are also defined in yml. 

Up to now, all checks are done within the YML data itself, we don't have to load anything else. This is the easy part done.

Next we check plugins. 

1 - YML defines plugins, ensure those plugins exist.
2 - We then pass the YML config for a given plugin to that plugin, so the plugin can run its own logic on what it expects. This gives plugin writers the opportunity to define and control their own state. IMPORTANT : plugin config checks must check config values only.

Next we check external systems. This is where things get really complicated. WBTB links to other servers and systems, like databases, source control servers, build servers, etc. Once again, we cannot use one system to help test another - we cannot use data stored in the database to test if a build server is online f.ex.

### Debug functions

It's useful to be able to call methods in a plugin directly for developing / debugging. You can do so with. To do so,

1. Ensure config is serialized and written to the plugin exe path to a file called `_init.txt`.

2. Write your method invocation args in a txt file with the following JSON structure 

    {
        "FunctionName":"UpdateUser",
        "Arguments":[
            {
                "Name":"my-parameter-name",
                "Value":{
                    // ... object or value for parameter
                }
            }
        ]
    }

3. Run

    dotnet plugin-dll-here --wbtb-interfaceCallFromFile your-args.txt

### Verify a plugin manually

you can verify a plugin manually by running

    dotnet plugin-dll-here --wbtb-handshake

## Extending

Basis for communicating with shell scripts

### Python

    import json
    import sys
    import base64

    # convert base64 string back to json, then convert to object
    obj = json.loads(base64.b64decode(sys.argv[1]).decode('utf-8'))

    # do something with object
    obj["Name"] = "changed it"

    # convert object back to JSON, then to a base64 string
    result = base64.b64encode(json.dumps(obj).encode()).decode('utf-8')

    # return result via stdout
    print(result)


