# Wbtb.Extensions.BuildServer.Jenkins

Plugin for connecting Wbtb to a Jenkins server.

## Config

    Plugins:
    -   Key: myJenkins
        Path: /var/wbtb/Wbtb.Extensions.BuildServer.Jenkins
    
    BuildServers:
    -   Plugin: myJenkins
        Config:
        - Host: http://myJenkins.com
        - Username: myUser
        - Token: Myaccesstoken
        Jobs:
        - Key: MyJob
          Config:
           - RemoteKey: My%20Build%20On%20Jenkins/
