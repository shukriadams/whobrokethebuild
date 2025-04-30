
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
        Jobs:
        -    Key: myjob
             SourceServer: mySourceControlServer

## Intervals

Data can be partitioned by intervals, or pooled into a single interval. The default is pooled, which lives in JSON/all

To use partitioning, create a file in your application binary directory called `.interval.Wbtb.Extensions.BuildServer.JenkinsSandbox.JenkinsSandbox.txt`, and in this add a single digit for the interval 
you want to read from. Interval directories are in JSON/_<interval>. The leading underscore is there because C# reflection doesn't support directories that start with numeric characters.

