using System;

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
            Console.WriteLine("Base diagnose mode reached. Override this method in your plugin to add more detailed testing.");
        }
    }
}
