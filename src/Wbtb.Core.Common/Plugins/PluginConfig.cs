using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Wbtb.Core.Common
{
    public class PluginConfig : IIdentifiable
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
        /// Key-value config specific to each plugin. These should be set to match what a given plugin expects.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Config { get; set; }

        /// <summary>
        /// Free-form complex config when key-value Config is not enough.
        /// </summary>
        public object ConfigComplex { get; set; }

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

        #region METHODS

        /// <summary>
        /// Parses raw JSON of plugin config to some type that can be passed in.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Response<T> Deserialize<T>(string propertyName)
        {
            using var jDoc = JsonDocument.Parse(this.RawJson);

            JsonElement configNode; 
            bool exists = jDoc.RootElement.TryGetProperty(propertyName, out configNode);
            if (!exists)
                return new Response<T> { 
                    Error = $"Expected config element '{propertyName}' not found" 
                };

            try
            {
                return new Response<T>
                {
                    Value = configNode.Deserialize<T>()
                };
            }
            catch (Exception ex)
            {
                return new Response<T>
                {
                    Error = $"Could not interpret JSON {configNode} : {ex}"
                };
            }
        }

        #endregion
    }
}
