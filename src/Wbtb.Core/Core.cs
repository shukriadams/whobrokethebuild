﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    public delegate void ShutdownHandler();

    public class Core
    {
        /// <summary>
        /// Updates config from git. Requires env variables to do so, self-checks and throws errors on missing config.
        /// Returns true if config has changed
        /// </summary>
        /// <returns></returns>
        public static bool EnsureConfig()
        {
            SimpleDI di = new SimpleDI();
            ConfigBootstrapper configBootstrapper = di.Resolve<ConfigBootstrapper>();
            CustomEnvironmentArgs customEnvironmentArgs = di.Resolve<CustomEnvironmentArgs>();
            customEnvironmentArgs.Apply();

            return configBootstrapper.EnsureLatest();
        }

        /// <summary>
        /// Single-call wrapper to start server.
        /// </summary>
        public static void StartServer(IEnumerable<string> allowedInternalPlugins)
        {
            // pre-start stuff
            SimpleDI di = new SimpleDI();
            ConfigBootstrapper configBootstrapper = di.Resolve<ConfigBootstrapper>();
            CustomEnvironmentArgs customEnvironmentArgs = di.Resolve<CustomEnvironmentArgs>();

            customEnvironmentArgs.Apply();
            configBootstrapper.EnsureLatest();

            // first part of server start, tries to load config
            ConfigurationManager.AllowedInternalPlugins = allowedInternalPlugins;
            Config unsafeConfig = ConfigurationManager.LoadUnsafeConfig(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.yml"));

            // ensure directories, this requires that config is loaded
            Directory.CreateDirectory(unsafeConfig.DataDirectory);
            Directory.CreateDirectory(unsafeConfig.BuildLogsDirectory);
            Directory.CreateDirectory(unsafeConfig.PluginsWorkingDirectory);
            Directory.CreateDirectory(unsafeConfig.PluginDataPersistDirectory);
            
            ConfigurationManager.FetchPlugins(unsafeConfig);
            ConfigurationManager.FinalizeConfig(unsafeConfig);

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
        }

        public static void LoadPlugins()
        {
            SimpleDI di = new SimpleDI();
            PluginManager pluginManager = di.Resolve<PluginManager>();
            ConfigurationBuilder builder = di.Resolve<ConfigurationBuilder>();

            pluginManager.Initialize();
            pluginManager.WriteCurrentPluginStateToStore();

            builder.InjectSourceServers();
            builder.InjectUsers();

            // build servers should be scafolded last, as they have data that has dependencies on other top level objects
            builder.InjectBuildServers();

            IEnumerable<string> orphans = builder.FindOrphans();
            foreach (string orphan in orphans)
                Console.WriteLine(orphan);

            if (orphans.Count() > 0)
                throw new ConfigurationException("Orphan records detected. Please merge or delete orphans");
        }
    }

}
