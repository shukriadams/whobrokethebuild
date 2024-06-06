using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    /// <summary>
    /// Wraps logic for starting a Wbtb application - both CLI and server. An application needs to be started in stages. 
    /// 1 - registered known basic types with DI system. These types will be used to start the app.
    /// 2 - Attempt to fetch 
    /// </summary>
    public class Core
    {
        /// <summary>
        /// Single-call wrapper to start server.
        /// </summary>
        public void Start(bool persistStateToDatabase=true, bool validate=true, bool verbose=true)
        {
            // pre-start stuff
            SimpleDI di = new SimpleDI();

            di.Register<UrlHelper, UrlHelper>();
            di.Register<ConfigurationLoader, ConfigurationLoader>();
            di.Register<CurrentVersion, CurrentVersion>();
            di.Register<LogHelper, LogHelper>();
            di.Register<PluginDirectSender, PluginDirectSender>();
            di.Register<PluginShellSender, PluginShellSender>();
            di.Register<PluginCoreSender, PluginCoreSender>();
            di.Register<PersistPathHelper, PersistPathHelper>();
            di.Register<MessageQueueHtppClient, MessageQueueHtppClient>();
            di.Register<ConfigurationBasic, ConfigurationBasic>();
            di.Register<ConfigurationBootstrapper, ConfigurationBootstrapper>();
            di.Register<GitHelper, GitHelper>();
            di.Register<ConfigurationBuilder, ConfigurationBuilder>();
            di.Register<PluginProvider, PluginProvider>();
            di.Register<PluginManager, PluginManager>();
            di.Register<FileSystemHelper, FileSystemHelper>();
            di.Register<CustomEnvironmentArgs, CustomEnvironmentArgs>();
            di.Register<Cache, Cache>();
            //di.RegisterFactory<ILogger, LogProvider>();
            di.RegisterFactory<IPluginSender, PluginSenderFactory>();

            // fetch latest config from git. Requires env vars set. Do after c
            ConfigurationBootstrapper configBootstrapper = di.Resolve<ConfigurationBootstrapper>();
            configBootstrapper.EnsureLatest(verbose);

            // NOTE : Always resolve ConfigurationBasic after apply custom env args, this class contains bootstrap settings needed to load app,
            // and these can be influenced by custom args
            ConfigurationBasic configBasic = di.Resolve<ConfigurationBasic>();
            string configPath = configBasic.ConfigPath;

            // first part of server start, try to load config
            ConfigurationLoader configurationManager = di.Resolve<ConfigurationLoader>();
            Configuration unvalidatedConfig = configurationManager.LoadUnvalidatedConfig(configPath, verbose);
            
            // it's safe to use datarootpath here, this doesn't need to be validated
            string cachePath = Path.Join(unvalidatedConfig.DataRootPath, ".currentConfigHash");

            if (File.Exists(cachePath)) 
            {
                try
                {
                    if (File.ReadAllText(cachePath) == unvalidatedConfig.Hash) 
                    {
                        validate = false;
                        if (verbose)
                            ConsoleHelper.WriteLine("Skipping config validation, config unchanged since last check.");
                    }
                        
                }
                catch (Exception ex) 
                {
                    ConsoleHelper.WriteLine($"Failed to read config hash : {ex.Message}");
                }
            }

            if (validate)
                configurationManager.EnsureNoneManifestLogicValid(unvalidatedConfig);

            // ensure directories, this requires that config is loaded
            Directory.CreateDirectory(unvalidatedConfig.DataRootPath);
            Directory.CreateDirectory(unvalidatedConfig.BuildLogsDirectory);
            Directory.CreateDirectory(unvalidatedConfig.PluginsWorkingDirectory);
            Directory.CreateDirectory(unvalidatedConfig.PluginDataPersistDirectory);

            configurationManager.FetchPlugins(unvalidatedConfig);
            configurationManager.LoadManifestData(unvalidatedConfig, verbose);
            configurationManager.ConfigValidated(unvalidatedConfig);
            
            try
            {
                File.WriteAllText(cachePath, unvalidatedConfig.Hash);
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine($"Failed to write config hash : {ex.Message}");
            }


            bool isAnyPluginProxying = unvalidatedConfig.Plugins.Where(p => p.Proxy).Any();
            if (isAnyPluginProxying || unvalidatedConfig.ForceMessageQueue)
            {
                MessageQueueHtppClient client = di.Resolve<MessageQueueHtppClient>();
                client.EnsureAvailable();
                client.AddConfig(unvalidatedConfig);
            }
            else
            {
                if (verbose)
                    ConsoleHelper.WriteLine("No plugins running in proxy mode, ignoring MessageQueue status.");
            }

            Configuration config = di.Resolve<Configuration>();

            // register plugins using config
            foreach (PluginConfig plugin in config.Plugins.Where(p => p.Manifest.RuntimeParsed == Runtimes.dotnet))
            {
                Type interfaceType = TypeHelper.GetCommonType(plugin.Manifest.Interface);

                if (!plugin.Proxy)
                    TypeHelper.GetAssembly(plugin.Manifest.Assembly); // force load assembly

                Type implementation = plugin.Proxy ? TypeHelper.GetRequiredProxyType(interfaceType) : TypeHelper.ResolveType(plugin.Manifest.Concrete);
                if (implementation == null)
                    throw new ConfigurationException($"Could not resolve plugin type {plugin.Manifest.Concrete}");

                PluginBehaviourAttribute pluginBehaviour = TypeHelper.GetAttribute<PluginBehaviourAttribute>(interfaceType);

                di.Register(interfaceType, implementation, key: plugin.Key, allowMultiple: pluginBehaviour.AllowMultiple);
            }

            PluginManager pluginManager = di.Resolve<PluginManager>();
            pluginManager.Initialize(validate);

            if (persistStateToDatabase)
                using (ConfigurationBuilder builder = di.Resolve<ConfigurationBuilder>()) 
                {
                    builder.TransactionStart();
                    pluginManager.WriteCurrentPluginStateToStore();

                    builder.InjectSourceServers();
                    builder.InjectUsers();

                    // build servers should be scafolded last, as they have data that has dependencies on other top level objects
                    builder.InjectBuildServers();

                    IEnumerable<string> orphans = builder.FindOrphans();
                    if (verbose)
                        foreach (string orphan in orphans)
                            ConsoleHelper.WriteLine(orphan);

                    if (config.FailOnOrphans && orphans.Count() > 0)
                        throw new ConfigurationException("Orphan records detected. Please merge or delete orphans. Disable this check with \"FailOnOrphans: false\" in config.");

                    builder.TransactionCommit();
                }
        }
    }
}
