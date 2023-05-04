using System.IO;

namespace Wbtb.Core.Common
{
    public class PersistPathHelper
    {
        #region FIELDS

        private readonly Config _config;

        #endregion

        #region

        public PersistPathHelper(Config config) 
        {
            _config = config;
        }

        #endregion

        #region METHODS

        public string GetPath(Plugin plugin, string itemFragment)
        {
            return Path.Combine(_config.PluginDataPersistDirectory, plugin.ContextPluginConfig.Key, itemFragment);
        }

        public string GetPath(Plugin plugin, string itemFragment, string itemFragment2)
        {
            return Path.Combine(_config.PluginDataPersistDirectory, plugin.ContextPluginConfig.Key, itemFragment, itemFragment2);
        }

        public string GetPath(Plugin plugin, string itemFragment, string itemFragment2, string itemFragment3)
        {
            return Path.Combine(_config.PluginDataPersistDirectory, plugin.ContextPluginConfig.Key, itemFragment, itemFragment2, itemFragment3);
        }

        public string GetPath(Plugin plugin, string itemFragment, string itemFragment2, string itemFragment3, string itemFragment4)
        {
            return Path.Combine(_config.PluginDataPersistDirectory, plugin.ContextPluginConfig.Key, itemFragment, itemFragment2, itemFragment3, itemFragment4);
        }

        #endregion
    }
}
