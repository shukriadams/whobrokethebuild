namespace Wbtb.Core.Common.Plugins
{
    public enum PluginSourceTypes : int
    {
        None,           // Plugin will not be fetched. Use this if path location will be forced. For dev/debugging.
        Git,            // Checks out latest tag at git repo.
        HttpGetZip      // fetches zip, unpacks to internal plugin directory based on metadata file in zip
    }
}
