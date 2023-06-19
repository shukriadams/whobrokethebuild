## Remote Config Sync

Wbtb config can be synced from a git repo. This makes managing a Wbtb instance easier as config changes can be placed under source control, with all the benefits therewith. To enable this feature, add the following environment variables to your Wbtb instance before starting:

    WBTB_GIT_CONFIG_REPO_URL : <url for git repo>
    WBTB_GIT_CONFIG_REPO_BRANCH : <branch name> (optional)    

The git url given must provide full access to the repo in question, so it should point to a public repo, or more likely, include an access token to a private git repo. An example of such a value would be `https://myuser:abcd1234abcd1234abcd1234abcd1234@github.com/myuser/mw-wbtb-config.git`

You can specify an optional branch to fetch the config from, else the default branch will be used. Finally, the branch in question should contain a file called `config.yml` in its root.

Updates are synced at the commit level, that is to say, Wbtb will always pull the latest commit from the branch provided. 

## Orphan records

    FailOnOrphans: <bool> (default is true)

Wbtb will fail to start if orphan records are detected in the database. This mechanism is there to catch misconfiguruation issues that would otherwise go unnoticed. You can disable this check, at your own risk, by setting `FailOnOrphans` to `false`.

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

All objects defined in Wbtb static config have unique `Key` values. Keys should not be changed once config is defined and loaded, as they are written to database. You can use whatever strng you want for keys, as long as they make sense to you, though we recommend you use anonymous IDs such as GUIDs, and use `Name` properties for human-friendly labels.

### Changing keys

Should you need to rename a key, do not change that key value abruptly. Instead, create a `KeyPrev` property, set this value to your current key, change the `Key` property as desired, then load the config. Wbtb will automatically update records with your key change. If you fail to do this Wbtb will create a new record with your changed key, but leave existing records unchanged, and existing records will be detected as orphans. Orphans can be deleted, but all their child records will disapper too, resulting in potential data loss. 

If your forgot to use the `KeyPrev` method to update a key, you can still recovover orphans, using the appropriate `IDataLayer_Merge*` command for the type of orphan.

## Plugins

Plugins are defined under  `Plugins` node. `Key` and `Path` are required. 

    Plugins: 
    -   Key: p4Sandbox
        Path: /var/wbtb/Wbtb.Extensions.SourceServer.PerforceSandbox

WBTB ships with a few built-in plugins, but these are typically for sandboxing. To use these use the `Proxy: true` property on a plugin.

    -   Key: p4Sandbox
        Proxy: true
        Path: /var/wbtb/Wbtb.Extensions.SourceServer.PerforceSandbox

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

    RevisionAtBuildRegex : <string>. Regex.
    Regex useed to parse revision at time of build. Use for jobs that are not triggered by commits.
    Requires that your build script writes the build out to log using a format can be parsed back with the given regex. For example, have your build script get current revision nr on the code base being built, then echo this out to log with echo "<current-revision>1234</current-revision>". Then set add 

        `RevisionAtBuildRegex : "<current-revision>(.*)<\/current-revision>"`

    to your config. Wbtb will parse out `1234` from your build log and assign that revision to your build.

### Optional Job properties

#### Image

`Image` gives the job a thumbnail in dashboards. It can be an absolute URL visible to your WBTB server, or you can mount images anywhere into `/var/wbtb/Wbtb.Core.Web/wwwroot` on your container and add their relative paths. For example, an image mapped to `/var/wbtb/Wbtb.Core.Web/wwwroot/images/myimage.jpg` will have have property `Image:/images/myimage.jpg` in config.


## Secrets

Config.yml is stored in plaintext. You can store credentials as env vars on your host/container instance and reference them in config.yml as `"{{env.YOUR_VAR_NAME}}"` (quotes required).