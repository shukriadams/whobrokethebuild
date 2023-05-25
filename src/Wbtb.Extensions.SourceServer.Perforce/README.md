# Wbtb.Extensions.BuildServer.Perforce

Plugin for connecting Wbtb to a Perforce server.

## Config

    Plugins:
    -   Key: p4
        Path: /var/wbtb/Wbtb.Extensions.SourceServer.Perforce

    SourceServers:
    -   Key: myPerforce
        Plugin: p4
        Config:
        - Host: myp4.com:1666
        - User: myuser
        - Password: mypassword
    
    BuildServers:
    -   Jobs:
        - SourceServer: myPerforce
          

