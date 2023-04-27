using System.IO;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    public class PersistPathHelper
    {
        public static string GetPath(Plugin plugin, string itemFragment)
        {
            return Path.Combine(ConfigKeeper.Instance.PluginDataPersistDirectory, plugin.ContextPluginConfig.Key, itemFragment);
        }

        public static string GetPath(Plugin plugin, string itemFragment, string itemFragment2)
        {
            return Path.Combine(ConfigKeeper.Instance.PluginDataPersistDirectory, plugin.ContextPluginConfig.Key, itemFragment, itemFragment2);
        }

        public static string GetPath(Plugin plugin, string itemFragment, string itemFragment2, string itemFragment3)
        {
            return Path.Combine(ConfigKeeper.Instance.PluginDataPersistDirectory, plugin.ContextPluginConfig.Key, itemFragment, itemFragment2, itemFragment3);
        }

        public static string GetPath(Plugin plugin, string itemFragment, string itemFragment2, string itemFragment3, string itemFragment4)
        {
            return Path.Combine(ConfigKeeper.Instance.PluginDataPersistDirectory, plugin.ContextPluginConfig.Key, itemFragment, itemFragment2, itemFragment3, itemFragment4);
        }

    }
}
