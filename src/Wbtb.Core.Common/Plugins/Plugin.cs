namespace Wbtb.Core.Common
{
    public abstract class Plugin
    {
        public string ProxyId { get; set; }

        public PluginConfig ContextPluginConfig { get; set; }

        /// <summary>
        /// Override as needed
        /// </summary>
        public void Diagnose()
        {

        }
    }
}
