# Wbtb.Extensions.BuildServer.Jenkins

Plugin for connecting Wbtb to a Jenkins server.

## Config

    Plugins:
    -   Id: myJenkins
        Path: /var/wbtb/Wbtb.Extensions.BuildServer.JenkinsSandbox   
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