using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Wbtb.Core.Common;
using YamlDotNet.Serialization;

namespace Wbtb.Core
{
    public class PluginManager
    {
        #region PROPERTIES

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;
        #endregion

        #region CTORS

        public PluginManager(PluginProvider pluginProvider, Configuration config) 
        {
            _pluginProvider = pluginProvider;
            _config = config;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Does all plugin init / setup / test / install etc. Call this from web start
        /// </summary>
        public void Initialize()
        {
            IList<PluginConfig> loadingPlugins = new List<PluginConfig>();
            
            // find this assembly using a type we know is defined in it
            Assembly commonAssembly = typeof(IPlugin).Assembly;

            // On a high level, the plugin manager needs to load plugins that are remote executables, but which can
            // also be simple local included projects. When loading remote executables, more checks are required, but
            // whereever possible, we try to use common logic to simplify flow and ensure pathways are well-tested

            // Checks :
            // 1 - does the executable exist. Remote only, will always if running locally
            // 2 - Try to get manifest data from plugin, we could read it directly using its path, but for common
            // useage we call a method on plugin to request its manifest as a string. This requires a plugin
            // instance
            foreach (PluginConfig pluginConfig in _config.Plugins)
            { 
                try 
                {
                    // get instance of plugin - this will either be a proxy to a remote executable, or if in development mode a local project assembly
                    // to use proxy proxying must be enabled, and the plugin must define a path where it can found
                    IPlugin plugin = _pluginProvider.GetByKey(pluginConfig.Key) as IPlugin;

                    if (pluginConfig.Proxy && !Directory.Exists(pluginConfig.Path))
                    {
                        // try to autoresolve if running in visual studio
                        string devPath = Path.Combine(pluginConfig.Path, "bin", "Debug", "net6.0");
                        if (Directory.Exists(devPath))
                        {
                            Console.WriteLine($"plugin location automatically remapped from {pluginConfig.Path} to {devPath}");
                            pluginConfig.Path = devPath;
                        } 
                        else 
                        {
                            Console.WriteLine($"Plugin '{pluginConfig.Key}' expected at path '{pluginConfig.Path}' but was not found.");
                            pluginConfig.Enable = false;
                            continue;
                        }
                    }

                    loadingPlugins.Add(pluginConfig);
                }
                catch (Exception ex)
                {
                    // plugin load should not throw unhandled exceptions, if it does, allow app to fail.
                    Console.WriteLine($"Plugin '{pluginConfig.Key}' threw exception on load {ex}");
                    pluginConfig.Enable = false;
                    throw ex;
                }
            }

            // once all plugin config loaded ...
            foreach (PluginConfig config in loadingPlugins)
            {
                // ensure plugin id set
                if (string.IsNullOrEmpty(config.Manifest.Key))
                    throw new ConfigurationException($"type {config.Path} has invalid metadata - no name defined");

                // ensure plugin id unique
                if (loadingPlugins.Where(r => r.Manifest.Key == config.Manifest.Key).Count() > 1)
                    throw new ConfigurationException($"multiple plugins with id {config.Manifest.Key} found");

                // parse inteface type
                Type interfaceFace = commonAssembly.GetType(config.Manifest.Interface);

                if (interfaceFace == null)
                    throw new ConfigurationException($"The manifest in plugin {config.Manifest.Key} declares an interface {config.Manifest.Interface} that could not be matched to a known interface. Please contact plugin developer.");

                // ensure exclusivity
                PluginBehaviourAttribute behaviourAttribute = TypeHelper.GetAttribute<PluginBehaviourAttribute>(interfaceFace);
                if (behaviourAttribute == null)
                    throw new ConfigurationException($"plugin interface {interfaceFace} does not impliment attribute {typeof(PluginBehaviourAttribute).Name}.");

                if (!behaviourAttribute.AllowMultiple){
                    IEnumerable<PluginConfig> implimenting = loadingPlugins.Where(r => r.Manifest.Interface== config.Manifest.Interface);
                    if (implimenting.Count() > 2)
                        throw new ConfigurationException($"multiple plugins ({string.Join(",", implimenting)}) declare interface {config.Manifest.Interface}, but this interface allows only a single active plugin");
                }
            }


            // once handshaked, need to initialize each plugin by passing config to it. this config contains data for all plugins so each can talk direcly, this requires
            // that all handsaking is done.
            // plugin init fail would put app into broken state, so must fail if any plugin fails
            foreach(PluginConfig pluginConfig in _config.Plugins)
            {
                IPlugin plugin = _pluginProvider.GetByKey(pluginConfig.Key) as IPlugin;

                PluginInitResult initResult;

                try
                {
                    initResult = plugin.InitializePlugin();
                }
                catch (Exception ex)
                {
                    // plugin init should not throw unhandled exceptions, if it does, allow app to fail.
                    Console.WriteLine($"Plugin '{pluginConfig.Key}' failed to load : {ex}");
                    pluginConfig.Enable = false;
                    throw ex;
                }

                if (initResult == null || !initResult.Success)
                    throw new ConfigurationException($"Plugin '{pluginConfig.Manifest.Key}' failed to initialize - {initResult.Description}");
            }

            // attempt to reach remote system behind plugin if plugin supports 
            foreach (PluginConfig pluginConfig in _config.Plugins)
            {
                IPlugin plugin = _pluginProvider.GetByKey(pluginConfig.Key) as IPlugin;
                if (typeof(IReachable).IsAssignableFrom(plugin.GetType()))
                {
                    IReachable reachable = plugin as IReachable;
                    ReachAttemptResult reach = reachable.AttemptReach();

                    // Todo : need more granularity here, we probably don't want to abort WBTB start just because a service is not available
                    if (reach.Reachable)
                        Console.WriteLine($"Plugin \"{pluginConfig.Key}\" is reachable.");
                    else
                        throw new ConfigurationException($"Credentials for plugin \"{pluginConfig.Key}\" failed : {reach.Error}{reach.Exception}.");
                }
            }

            // attempt to initialize datastores for all datalayer plugins
            foreach (PluginConfig pluginConfig in _config.Plugins.Where(p => p.Manifest.Interface == TypeHelper.Name<IDataPlugin>()))
            {
                IDataPlugin dataLayerPlugin = _pluginProvider.GetDistinct(pluginConfig) as IDataPlugin;
                dataLayerPlugin.InitializeDatastore();
            }


            // validate build servers
            foreach (BuildServer buildserver in _config.BuildServers)
            {
                IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildserver.Plugin) as IBuildServerPlugin;

                try 
                {
                    buildServerPlugin.AttemptReach(buildserver);
                }
                catch(Exception ex)
                {
                    throw new ConfigurationException($"Lookup for build server \"{buildserver.Name}\" failed:{ex}");
                }

                foreach (Job job in buildserver.Jobs)
                {
                    try
                    {
                        buildServerPlugin.VerifyBuildServerConfig(buildserver);
                        buildServerPlugin.AttemptReachJob(buildserver, job);
                    }
                    catch(Exception ex)
                    {
                        throw new ConfigurationException($"Lookup for job \"{job.Name}\" failed:{ex}");
                    }
                }
            }

            // validate source servers
            foreach (SourceServer sourceServer in _config.SourceServers) 
            {
                ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;

                try
                {
                    sourceServerPlugin.AttemptReach(sourceServer);
                    Console.WriteLine($"{sourceServer.Name} is contactable");
            
                    // verify config
                    sourceServerPlugin.VerifySourceServerConfig(sourceServer);

                    foreach (BuildServer buildServer in _config.BuildServers)
                        foreach (Job job in buildServer.Jobs)
                            sourceServerPlugin.VerifyJobConfig(job);
                            
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException($"Source server \"{sourceServer.Key}\" failed config test :{ex}");
                }
            }

            // validate alert config against plugins - alerting is done by a specific plugin, that plugin can impose its own requirements.
            foreach (BuildServer buildserver in _config.BuildServers)
                foreach (Job job in buildserver.Jobs)
                {
                    foreach(MessageHandler alert in job.Message)
                    {
                        IPlugin plugin = _pluginProvider.GetByKey(alert.Plugin);
                        if (!typeof (IMessagingPlugin).IsAssignableFrom(plugin.GetType()))
                            throw new ConfigurationException($"Job \"{job.Key}\" defines an alert with plugin \"{alert.Plugin}\". This plugin exists, but does not impliment the interface {typeof(IMessagingPlugin).FullName}.");
                    
                        int configCount = 0;
                        if (!string.IsNullOrEmpty(alert.User))
                            configCount ++;
                        if (!string.IsNullOrEmpty(alert.Group))
                            configCount++;

                        if (configCount == 0)
                            throw new ConfigurationException($"Job \"{job.Key}\" has no target config - define either User, Group or Config info.");

                        if (configCount > 1)
                            throw new ConfigurationException($"Job \"{job.Key}\" defines more than one target config - use only User, Group or Config info.");
                        
                        MessageConfiguration config = null;

                        // alerts are chained from job > user|group > alert handling plugin
                        if (!string.IsNullOrEmpty(alert.User))
                        { 
                            User user = _config.Users.FirstOrDefault(u => u.Key == alert.User);
                            if (user == null)
                                throw new ConfigurationException($"Job \"{job.Key}\" defines a target user \"{alert.User}\" but this user is not defined under users.");

                            config = user.Message.FirstOrDefault(r => r.Plugin == alert.Plugin);
                            if (config == null)
                                throw new ConfigurationException($"Job \"{job.Key}\" defines a target user \"{alert.User}\" with expected plugin info for \"{alert.Plugin}\", but this user has no config for this plugin.");
                        }

                        if (!string.IsNullOrEmpty(alert.Group))
                        {
                            Group group = _config.Groups.FirstOrDefault(u => u.Key == alert.Group);
                            if (group == null)
                                throw new ConfigurationException($"Job \"{job.Key}\" defines a target group \"{alert.Group}\" but this group is not defined under groups.");

                            config = group.Message.FirstOrDefault(r => r.Plugin == alert.Plugin);
                            if (config == null)
                                throw new ConfigurationException($"Job \"{job.Key}\" defines a target group \"{alert.Group}\" with expected plugin info for \"{alert.Plugin}\", but this user has no config for this plugin.");

                        }
                        
                        IMessagingPlugin messagingPlugin = plugin as IMessagingPlugin;

                        try
                        {
                            messagingPlugin.AttemptReach();
                        }
                        catch (ConfigurationException ex)
                        {
                            throw new ConfigurationException($"Plugin \"{alert.Plugin}\" failed reach attempt : {ex.Message}");
                        }

                        try 
                        {
                            messagingPlugin.ValidateAlertConfig(config); 
                        } 
                        catch (ConfigurationException ex)
                        { 
                            throw new ConfigurationException($"Plugin \"{alert.Plugin}\" rejected alert config for job \"{job.Key}\" : {ex.Message}");
                        }
                    }
                }

            // validate post processor config
            foreach (BuildServer buildserver in _config.BuildServers)
                foreach (Job job in buildserver.Jobs)
                    foreach (string postProcessor in job.PostProcessors) 
                    {
                        IPostProcessorPlugin postProcessorPlugin = _pluginProvider.GetByKey(postProcessor) as IPostProcessorPlugin;
                        postProcessorPlugin.VerifyJobConfig(job);
                    }

            ValidateRuntimeState();

            foreach (PluginConfig pluginConfig in _config.Plugins)
                Console.WriteLine($"WBTB : initialized plugin {pluginConfig.Key}");
        }

        private void ValidateRuntimeState()
        {
            // validate soft config, ie, that there is 1 data layer etc et
            if (!_config.Plugins.Any(plugin => plugin.Manifest.Interface == TypeHelper.Name<IDataPlugin>()))
                throw new ConfigurationException("ERROR : No active data plugin detected. Please ensure your application has a plugin of category 'data', and that it is enabled.");
        }

        public void WriteCurrentPluginStateToStore() 
        {
            // write unique config to db
            ISerializer serializer = YmlHelper.GetSerializer();
            string serializedConfig = serializer.Serialize(_config);
            string configHash = Sha256.FromString(serializedConfig);
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            ConfigurationState latestConfigState = dataLayer.GetLatestConfigurationState();
            if (latestConfigState == null || latestConfigState.Hash != configHash)
            {
                if (latestConfigState != null)
                    Console.WriteLine($"Warning - config has changed since last time. If current site loads due to config errors, check configurationstate table for ref. Storing new hash ({configHash}) of current config");

                // todo : need user prompt to force add new config state to db
                dataLayer.AddConfigurationState(new ConfigurationState
                {
                    Content = serializedConfig,
                    Hash = configHash,
                    CreatedUtc = DateTime.UtcNow
                });
            }
        }

        #endregion
    }
}
