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

## Keys

All objects defined in Wbtb static config have unique `Key` values. Keys should not be changed once config is defined and loaded, as they are written to database. 

### Changing keys

Do not change keys abruptly. Instead, create a `KeyPrev` property, set this value to your current key, change the `Key` property as desired, then load the config. Wbtb will automatically update records. If you fail to do this Wbtb will create a new record with your changed key, but leave existing records unchanged. The existing records will be detected as orphans. Orphans can be deleted, but all their child records will disapper too, resulting in potential data loss. 

If your forgot to use the `KeyPrev` method to update a key, you can still recovover orphans, using the appropriate `IDataLayerPlugin_Merge*` command for the type of orphan.

## Job

Jobs are tied to a build server. A job must have a unique `Key`, as well as valid `SourceServer` property.

    SourceServers:
    -   Key: myperforce

    BuildServers:
    -   Key : MyBuildServer
        Jobs:
        -   Key: Project_Ironbird
            SourceServer: myperforce

### Detailed job settings

    ImportCount : <int>. Default is 100.
    
    Number of builds to import for a given job.


### Optional Job properties

#### Image

`Image` gives the job a thumbnail in dashboards. It can be an absolute URL visible to your WBTB server, or you can mount images anywhere into `/var/wbtb/Wbtb.Core.Web/wwwroot` on your container and add their relative paths. For example, an image mapped to `/var/wbtb/Wbtb.Core.Web/wwwroot/images/myimage.jpg` will have have property `Image:/images/myimage.jpg` in config.


## Secrets

Config.yml is stored in plaintext. You can store credentials as env vars on your host/container instance and reference them in config.yml as `"{{env.YOUR_VAR_NAME}}"` (quotes required).