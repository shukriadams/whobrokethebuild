using System;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    public class PluginOutputEncoder
    {
        public static string[] Encode<TSourcePlugin>(object payload)
        {
            return Encode(payload, typeof(TSourcePlugin));
        }

        public static string[] Encode(object payload, IPlugin callingPlugin)
        {
            return Encode(payload, callingPlugin.GetType());
        }

        public static string[] Encode(object payload, Type callingPluginType)
        {
            return new string[]{
                $"<WBTB-output ts=\"{DateTime.UtcNow}\" pluginType=\"{callingPluginType.Namespace}.{callingPluginType.Name}\" >",
                JsonConvert.SerializeObject(payload),
                "</WBTB-output>"
            };
        }

    }
}
