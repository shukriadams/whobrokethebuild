## Config requirements

### Settings

    Plugins: 
    -   Key: myslack
        Path: Wbtb.Extensions.Messaging.Slack
        Config:
        - Token: <slack access token>

### General informing

To explicitly inform a user or group on every build regression/fix, using the following config. This doesn't treat the target as being implicated in the build.

    Users:
        - Key: some_user_key
          Message:
          - Plugin: myslack
            SlackId: myslackid

    Groups:
        - Key: some_group_key
          Message:
          - Plugin: myslackchannel
            SlackId: myslackchannelid

    BuildServers:
    -   <buildserver config>
        Jobs:
        -   Message:
            - Plugin: myslack
              User: some_user_key
            - Plugin: myslackchannel
              Group: some_group_key

### Build involvement alerting

To inform user that they are involved in a build break/fix

- the user needs declare their slack id in an `Alert` node, this Slack plugin requries the `SlackId` string value which is the User's public SlackId.
- the user must also define a Source server identity, this is the identity their commits will have, and this must be bound to an active source server.

    Users:
      - Key: MyUser
        Message:
        - Plugin: myslack
          SlackId: myslackid
        SourceServerIdentities:
        - Name: name_in_commits
          SourceServerKey: my_sourceServer

    SourceServers:
    -   Key: my_sourceServer

    BuildServers:
    -   Jobs:
        -   SourceServer: my_sourceServer


Messages sent in this way treat the target as being implicated in the build break.
