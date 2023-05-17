using System;

namespace Wbtb.Core.Common
{
    public class PluginOutputEncoder
    {
        public static string[] Encode<TSourcePlugin>(string messageId)
        {
            return Encode(messageId, typeof(TSourcePlugin));
        }

        public static string[] Encode(string messageId, IPlugin callingPlugin)
        {
            return Encode(messageId, callingPlugin.GetType());
        }

        public static string[] Encode(string messageId, Type callingPluginType)
        {
            return new string[]{
                $"<WBTB-output ts=\"{DateTime.UtcNow}\" pluginType=\"{callingPluginType.Namespace}.{callingPluginType.Name}\" >",
                messageId,
                "</WBTB-output>"
            };
        }

    }
}
