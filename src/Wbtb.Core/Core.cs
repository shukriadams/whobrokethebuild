using Microsoft.Extensions.Logging;
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
        public void Start(bool persistStateToDatabase=true)
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
            di.Register<BuildLogParseResultHelper, BuildLogParseResultHelper>();
            di.Register<ConfigurationBuilder, ConfigurationBuilder>();
            di.Register<PluginProvider, PluginProvider>();
            di.Register<PluginManager, PluginManager>();
            di.Register<FileSystemHelper, FileSystemHelper>();
            di.Register<CustomEnvironmentArgs, CustomEnvironmentArgs>();
            di.RegisterFactory<ILogger, LogProvider>();
            di.RegisterFactory<IPluginSender, PluginSenderFactory>();

            ConfigurationBootstrapper configBootstrapper = di.Resolve<ConfigurationBootstrapper>();
            CustomEnvironmentArgs customEnvironmentArgs = di.Resolve<CustomEnvironmentArgs>();
            customEnvironmentArgs.Apply();
            configBootstrapper.EnsureLatest();

            ConfigurationLoader configurationManager = di.Resolve<ConfigurationLoader>();

            // first part of server start, tries to load config
            // always resolve ConfigurationBasic after apply custom env args
            ConfigurationBasic configBasic = di.Resolve<ConfigurationBasic>(); 
            string configPath = configBasic.ConfigPath;
            Configuration unsafeConfig = configurationManager.LoadUnsafeConfig(configPath);

            // ensure directories, this requires that config is loaded
            Directory.CreateDirectory(unsafeConfig.DataDirectory);
            Directory.CreateDirectory(unsafeConfig.BuildLogsDirectory);
            Directory.CreateDirectory(unsafeConfig.PluginsWorkingDirectory);
            Directory.CreateDirectory(unsafeConfig.PluginDataPersistDirectory);

            configurationManager.FetchPlugins(unsafeConfig);
            configurationManager.FinalizeConfig(unsafeConfig);

            bool isAnyPluginProxying = unsafeConfig.Plugins.Where(p => p.Proxy).Any();
            if (isAnyPluginProxying || unsafeConfig.ForceMessageQueue)
            {
                MessageQueueHtppClient client = di.Resolve<MessageQueueHtppClient>();
                client.EnsureAvailable();
                client.AddConfig(unsafeConfig);
            }
            else
            {
                Console.WriteLine("No plugins running in proxy mode, ignoring MessageQueue status.");
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
            ConfigurationBuilder builder = di.Resolve<ConfigurationBuilder>();

            pluginManager.Initialize();

            if (persistStateToDatabase)
            {
                pluginManager.WriteCurrentPluginStateToStore();

                builder.InjectSourceServers();
                builder.InjectUsers();

                // build servers should be scafolded last, as they have data that has dependencies on other top level objects
                builder.InjectBuildServers();

                IEnumerable<string> orphans = builder.FindOrphans();
                foreach (string orphan in orphans)
                    Console.WriteLine(orphan);

                if (config.FailOnOrphans && orphans.Count() > 0)
                    throw new ConfigurationException("Orphan records detected. Please merge or delete orphans. Disable this check with \"FailOnOrphans: false\" in config.");
            }
        }
    }
}
