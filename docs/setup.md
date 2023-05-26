## Daemon Interval

Interval daemons tick at. In seconds. Must be an integer. Default is 60 seconds.

    DaemonInterval : <int>

## Config Path

Config path can be optionally overridden with the env var `WBTB_CONFIGPATH`. This should be an absolute path, and must include file name. If not set, config path will fall back to the application startup directory + `config.yml`, which under normal circumstances will be the path where `Wbtb.Core.Web.dll` is located.

## Group config

To send alerts to abstract receivers like public Slack channels, use the "groups" concept


Users can also be grouped together to make it easier to send alerts to everyone based on role

    Users:
        - Key : JessMcAdmin
        - Key : SamAdminsen

    Groups:
        - Key: OurAdminGroup
          Users :
          - JessMcAdmin
          -  SamAdminsen