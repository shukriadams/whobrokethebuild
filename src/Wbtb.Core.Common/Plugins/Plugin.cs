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
            ConsoleHelper.WriteLine("Base diagnose mode reached. Override this method in your plugin to add more detailed testing. Use the --help switch for more options.");
        }
    }
}
