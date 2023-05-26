using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wbtb.Core.Common;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Wbtb.Core
{
    public class ConfigurationHelper
    {
        /// <summary>
        /// public converts any given YamlNode (which is itself a dynamic-type) to component JSON string. 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string YmlNodeToJson(YamlNode node)
        {
            // convert dyamic parsed YML to JSON
            YamlStream stream = new YamlStream { new YamlDocument(node) };
            using StringWriter writer = new StringWriter();
            stream.Save(writer);

            using (StringReader reader = new StringReader(writer.ToString()))
            {
                Deserializer jsonDeserializer = new Deserializer();
                object yamlObject = jsonDeserializer.Deserialize(reader);
                ISerializer serializer = new SerializerBuilder()
                    .JsonCompatible()
                    .Build();

                return serializer
                    .Serialize(yamlObject)
                    .Trim();
            }
        }

        /// <summary>
        /// Loads raw YML config as dynamic
        /// </summary>
        /// <param name="rawYml"></param>
        /// <returns></returns>
        public static YamlNode RawConfigToDynamic(string rawYml)
        {
            using (StringReader read = new StringReader(rawYml))
            {
                YamlStream yaml = new YamlStream();
                yaml.Load(read);
                if (!yaml.Documents.Any())
                    throw new ConfigurationException("Yaml config did not resolve to any nodes - config most likely empty.");

                return yaml.Documents.Single().RootNode;
            }
        }

        /// <summary>
        /// finds config for plugin, returns it as JSON string
        /// </summary>
        /// <param name="rawConfig"></param>
        /// <param name="pluginId"></param>
        /// <returns></returns>
        public static string GetRawPluginConfigByPluginId(YamlNode rawConfig, string pluginId)
        {
            if (!rawConfig.AllNodes.Contains("Plugins"))
                return null;

            foreach (YamlNode rawPluginConfig in (IEnumerable<YamlNode>)rawConfig["Plugins"])
            {
                if (!rawPluginConfig.AllNodes.Contains("Key"))
                    continue;

                string rawId = rawPluginConfig["Key"].ToString();
                if (pluginId == rawId)
                    return ConfigurationHelper.YmlNodeToJson(rawPluginConfig);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawConfig"></param>
        /// <param name="section">Users or Groups </param>
        /// <param name="objectId"></param>
        /// <param name="alertIndex"></param>
        /// <returns></returns>
        public static string GetRawAlertConfigByIndex(YamlNode rawConfig, string section, string objectId, int alertIndex)
        {
            foreach (YamlNode userConfig in (IEnumerable<YamlNode>)rawConfig[section])
            {
                if (!userConfig.AllNodes.Contains("Key"))
                    continue;

                string rawId = userConfig["Key"].ToString();

                if (objectId == rawId && userConfig.AllNodes.Contains("Message") && userConfig["Message"] != null)
                {
                    IEnumerable<YamlNode> alertConfigs = (IEnumerable<YamlNode>)userConfig["Message"];
                    if (alertConfigs == null)
                        throw new ConfigurationException($"Message config for user {objectId} could not be converted to IList");

                    if (alertConfigs.Count() > alertIndex)
                        return ConfigurationHelper.YmlNodeToJson(alertConfigs.ElementAt(alertIndex));
                }
            }

            return null;
        }
    }
}
