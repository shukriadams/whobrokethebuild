namespace Wbtb.Core.Common.Plugins
{
    public abstract class Plugin
    {
        public string ProxyId { get; set; }

        public PluginConfig ContextPluginConfig { get; set; }

        // override as needed
        public void InjectConfig(PluginConfig config)
        {
            // ensure variables here

            this.ContextPluginConfig = config;
        }
    }
}
