using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common.Configuration
{
    public class ConfigurationHelper
    {

        public static bool HasConfigItem(Config config, string pluginId, string itemName)
        {
            PluginConfig plugin = config.Plugins.FirstOrDefault(r => r.Key == pluginId);
            if (plugin == null)
                return false;

            if (plugin.Config == null)
                return false;

            KeyValuePair<string, object>? item = plugin.Config.FirstOrDefault(c => c.Key == itemName);
            if (item == null)
                return false;

            if (item.Value.Value == null)
                return false;

            return true;
        }

        public static bool HasConfigItem(IEnumerable<IConfigHolder> items, string pluginId, string itemName)
        {
            IConfigHolder plugin = items.FirstOrDefault(r => r.Key == pluginId);
            if (plugin == null)
                return false;

            if (plugin.Config == null)
                return false;

            KeyValuePair<string, object>? item = plugin.Config.FirstOrDefault(c => c.Key == itemName);
            if (item == null)
                return false;

            if (item.Value.Value == null)
                return false;

            return true;
        }

    }
}
