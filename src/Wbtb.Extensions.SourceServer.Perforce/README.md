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
        -   Host: host.domain:1666
        -   User: <string>
        -   Password: <string>
        -   TrustFingerprint : <string> (optional) 

    BuildServers:
    -   Jobs:
        -   SourceServer: myPerforce
            Config:
            -   p4depotRoot: //mydepot/mystream/...

Perforce integration in WBTB is stateless - you don't have to set any environment variables or P4 session for the WBTB host environment.

`TrustFingerprint` is required for SSL servers only, is the fingerprint of your SSL Key, and looks like `xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx`. You can obtain it by running `p4 trust -l` against your Perforce server. See
https://www.perforce.com/manuals/cmdref/Content/CmdRef/p4_trust.html for more info.

`p4depotRoot` is the path to the stream root a given job will read code from.

## Defining users

To add a user MyUser with a Perforce username `p4_myUser` :
          
    Users:
    -   Key: myUser
        SourceServerIdentities:
            -   SourceServerKey: myPerforce
                Name: p4_myUser

