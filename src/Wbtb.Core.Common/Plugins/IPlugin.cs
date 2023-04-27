using System;

namespace Wbtb.Core.Common.Plugins
{
    public interface IPlugin
    {
        /// <summary>
        /// If the plugin needs to do work before it can run. Called on app start.
        /// </summary>
        /// <returns></returns>
        PluginInitResult InitializePlugin();

        /// <summary>
        /// NO LONGER USED
        /// Injects instance-specific config into plugin. Egs, a perforce plugin can take talk to multiple servers, for each server
        /// you'll need a unique plugin instance, this method passes the confic for that server to the instance. passing is done 
        /// for you in the plugin provider.
        /// </summary>
        [Obsolete]
        void InjectConfig(PluginConfig config);

        /// <summary>
        /// Config for this instance of plugin
        /// </summary>
        PluginConfig ContextPluginConfig { get;set; }
    }
}
