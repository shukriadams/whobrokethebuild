## Config requirements

### Settings

    Plugins: 
    -   Key: myslack
        Path: Wbtb.Extensions.Messaging.SlackSandbox
        Config:
        - Token: <fake slack access token>

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
          - Plugin: myslack
            SlackId: myslackchannelid
            IsGroup: true

    BuildServers:
    -   <buildserver config>
        Jobs:
        -   Key: myJob
            Message:
            - Plugin: myslack
              User: some_user_key
            - Plugin: myslack
              Group: some_group_key

Note the `IsGroup: true` for targeting groups. 