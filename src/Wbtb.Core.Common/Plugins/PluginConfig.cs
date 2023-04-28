using System.Collections.Generic;
using Wbtb.Core.Common.Configuration;

namespace Wbtb.Core.Common.Plugins
{
    public class PluginConfig : IIdentifiable, IConfigHolder
    {
        #region PROPERTIES

        /// <summary>
        /// Unique, required identifier for plugin. 
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string KeyPrev { get; set; }

        /// <summary>
        /// Config for this plugin, directly from the config yml file, in JSON form. This allows for any property to be added to the config and 
        /// be transferred to a plugin, without WBTB core requiring type definition of it.
        /// </summary>
        public string RawJson { get; set; }

        /// <summary>
        /// Optional. Human-friendly label this plugin provides.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Should be set internally.
        /// 
        /// If false, plugin is compiled with and runs in the same application context as Wbtb core application. If true, plugin is a shell app that runs 
        /// in its own context. Note that Proxy can still be enabled for internal plugins.
        /// </summary>
        public bool IsExternal { get; set; }

        /// <summary>
        /// Source enum plugin binaries pulled from. For external plugins only
        /// </summary>
        public PluginSourceTypes SourceType { get; set; }

        /// <summary>
        /// True by default, can be set to false in static config. If false, plugin config is completely ignored by wbtb
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// In proxy mode plugins function as external applications, communicating over JSON.
        /// Default: true
        /// </summary>
        public bool Proxy { get; set; }

        /// <summary>
        /// Dependenton SourceType. either url to ZIP, or filesystem path to ZIP.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Path this plugin can be executed at. 
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// metadata from plugin source once confirmed and loaded. Is null if 
        /// </summary>
        public PluginManifest Manifest { get; set; }

        /// <summary>
        /// Addition key-value config specific to each plugin. These should be set to match what a given plugin expects.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Config { get; set; }

        /// <summary>
        /// namespace+type name of Proxytype for this plugin. 
        /// </summary>
        public string ForcedProxyType { get; set; }

        #endregion

        #region CTORS

        public PluginConfig()
        {
            this.Enable = true;
            this.Proxy = true;
            this.Config = new List<KeyValuePair<string, object>>();
        }

        #endregion

    }
}
