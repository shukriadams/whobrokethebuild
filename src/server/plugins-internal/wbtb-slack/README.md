## Settings

In your WBTB settings file you can add the following settings for this plugin

    plugins:
        wbtb-slack:

            # Required. Slack API access token
            accessToken: {string}

            # optional. Default false. When true, the plugin will read from and write to internal shims, obviating the need for an actual
            # Slack API to interact with. Use this for local dev and testing.
            sandboxMode: {boolean}

            # optional. Slack user id. If set, all slack messages will be diverted to this user. Use for testing.
            overrideUserId: {string}

            # optional. Slack channel id. If set, all group slack messages will be diverted to this channel. Use for testing.
            overrideChannelId : {string}