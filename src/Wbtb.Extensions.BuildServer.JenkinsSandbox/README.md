
A mock build server lets us simulate build events during development time. 

## How to use

You will need to couple this plugin with a version control plugin of some kind, as a build job expects a source control server. WBTB has a built in mock Perforce server that can be used for this.

In config add 

    Plugins:
    -   Key: jenkinsSandbox
        Path: Wbtb.Extensions.BuildServer.JenkinsSandbox

    BuildServers:
    -   Key: MyJenkins
        Plugin: jenkinsSandbox
        Config:
        -   Interval: 1 
        Jobs:
        -    Key: myjob
             SourceServer: mySourceControlServer

Note the config interval value. This lets us control "time" in our test data
