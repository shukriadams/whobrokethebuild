
mocking build server is most important part of wbtb as builds drrive all events.

## How to use

You will need a real version control system
In core config add 

    Plugins:
    -   Id: fakeJenkins
        Path: C:\projects\wbtb\src\Wbtb.Extensions.BuildServer.JenkinsSandbox   
        Proxy: false
    -   Id: WBTB-Perforce
        Path: C:\projects\wbtb\src\Wbtb.Extensions.SourceServer.Perforce
        Proxy: false
        Enable: true

    BuildServers:
    -   Id: MyJenkins
        Host: jenkins.myserver.local
        Plugin: fakeJenkins
        Enable: true
        Jobs:
        -    Id: myjob
             SourceServer: p4_1
             Enable: false

    SourceServers:
    -   Id: p4_1
        Host: ssl:p4.myserver.local:1666
        User: myuser
        Password: myPassword