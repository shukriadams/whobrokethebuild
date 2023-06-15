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
        -   Host: myp4.com:1666
        -   User: myuser
        -   Password: mypassword
        -   Trust : true|false (optional) 

    BuildServers:
    -   Jobs:
        -   SourceServer: myPerforce

## Defining users

To add a user MyUser with a Perforce username `p4_myUser` :
          
    Users:
    -   Key: myUser
        SourceServerIdentities:
            -   SourceServerKey: myPerforce
                Name: p4_myUser

