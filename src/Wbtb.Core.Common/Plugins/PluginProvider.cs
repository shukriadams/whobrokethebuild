using System;
using System.Linq;

namespace Wbtb.Core.Common
{
    public class PluginProvider
    {
        private readonly Config _config;
             
        public PluginProvider() 
        {
            SimpleDI di = new SimpleDI();
            _config = di.Resolve<Config>(); 
        }

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

            PluginConfig config = _config.Plugins.FirstOrDefault(p => p.Key == key);
            if (config == null)
                throw new Exception($"Requested plugin {key} does not exist or is disabled. This error should not occur, and is likely caused by config that has been improperly changed. Do a scan for orphaned records and fix accordingly.");

            return GetDistinct(config) as IPlugin;
        }

        public object GetDistinct(PluginConfig pluginConfig, bool forceConcrete = true)
        {
            if (pluginConfig == null)
                return null;

            object plugin;

            // normally in production environments, factory is the proxy factory. in dev environments your starting app needs to supply it's own factory, normally done
            // with ninject, unity or some other other IOC system, as well as statically include the plugin projects you intend to load.
            if (pluginConfig.Proxy || _config.IsCurrentContextProxyPlugin)
            {
                Type concreteType = Type.GetType(pluginConfig.ForcedProxyType, true, true);
                if (concreteType == null)
                    throw new Exception($"Could not resolve expected type {pluginConfig.ForcedProxyType}");

                plugin = Activator.CreateInstance(concreteType);
                ((IPluginProxy)plugin).PluginKey = pluginConfig.Key;
            }
            else 
            {
                if (forceConcrete) 
                {
                    // TODO - cache type lookup for performance
                    Type? concreteType = TypeHelper.ResolveType(pluginConfig.Manifest.Concrete);
                    if (concreteType == null)
                        throw new ConfigurationException($"Could not load concrete type {pluginConfig.Manifest.Concrete} from available assemblies");

                    SimpleDI di = new SimpleDI();
                    plugin = di.ResolveImplementation(concreteType);
                    if (plugin == null)
                        throw new ConfigurationException($"Could not create instance of plugin {pluginConfig.Key} by concrete type {pluginConfig.Manifest.Concrete}.");
                } 
                else 
                {
                    // get by interface
                    SimpleDI di = new SimpleDI();
                    plugin = di.Resolve(TypeHelper.ResolveType(pluginConfig.Manifest.Interface));
                    if (plugin == null)
                        throw new ConfigurationException($"Could not create instance of plugin {pluginConfig.Key} by interface {pluginConfig.Manifest.Interface}.");
                }
            }

            IPlugin iplugin = plugin as IPlugin;

            // equivalent when proxying is done in PluginReceiver
            iplugin.InjectConfig(pluginConfig);

            return plugin;
        }
    }
}
