using System;
using System.Linq;

namespace Wbtb.Core.Common
{
    public class PluginProvider
    {
        #region FIELDS

        private readonly Configuration _config;

        #endregion
        
        #region CTORS

        public PluginProvider() 
        {
            SimpleDI di = new SimpleDI();
            _config = di.Resolve<Configuration>(); 
        }

        #endregion

        #region METHODS

        public T GetFirstForInterface<T>(bool expected = true)
        { 
            Type t = typeof(T);

            PluginConfig config = _config.Plugins.FirstOrDefault(c => c.Manifest.Interface == $"Wbtb.Core.Common.{t.Name}");
            T plugin = (T)GetDistinct(config, false);
            
            if (expected && plugin == null)
                throw new PluginNotFoundExpection(typeof(T));

            return plugin; 
        }

        public IPlugin GetByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new Exception("Attemtping to get plugin ById, but id is empty. Error likely caused by upstream data error");

            PluginConfig config = _config.Plugins.FirstOrDefault(c => c.Key == key);

            SimpleDI di = new SimpleDI();
            IPlugin plugin = di.ResolveByKey<IPlugin>(key);
            
            // equivalent when proxying is done in PluginReceiver
            plugin.ContextPluginConfig = config;

            if (plugin is IPluginProxy)
                ((IPluginProxy)plugin).PluginKey = key;

            return plugin;
        }

        public TPluginType GetDistinct<TPluginType>()
        {
            PluginConfig config = _config.Plugins.FirstOrDefault(c => c.Manifest.Interface == TypeHelper.Name<TPluginType>());
            if (config == null)
                throw new Exception($"No service found for type {TypeHelper.Name<TPluginType>()}");

            object resolved = GetDistinct(config);

            TPluginType resolvedTyped = (TPluginType)resolved;
            if (resolvedTyped == null)
                throw new Exception($"Could not cast plugin {config.Key} to {TypeHelper.Name<TPluginType>()}");

            return resolvedTyped;
        }
        public object GetDistinct(PluginConfig pluginConfig, bool forceConcrete = true)
        {
            if (pluginConfig == null)
                return null;

            object plugin;
            SimpleDI di = new SimpleDI();
            plugin = di.Resolve(TypeHelper.ResolveType(pluginConfig.Manifest.Interface));
            if (plugin == null)
                throw new ConfigurationException($"Could not create instance of plugin {pluginConfig.Key} by interface {pluginConfig.Manifest.Interface}.");
            
            IPlugin iplugin = plugin as IPlugin;

            // equivalent when proxying is done in PluginReceiver
            iplugin.ContextPluginConfig = pluginConfig;

            if (iplugin is IPluginProxy)
                ((IPluginProxy)plugin).PluginKey = pluginConfig.Key;

            return plugin;
        }

        #endregion
    }
}
