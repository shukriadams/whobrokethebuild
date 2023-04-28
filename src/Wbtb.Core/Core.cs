﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Configuration;

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
            CustomEnvironmentArgs.Apply();
            return ConfigBootstrapper.EnsureLatest();
        }

        /// <summary>
        /// Single-call wrapper to start server.
        /// </summary>
        public static void StartServer(IEnumerable<string> allowedInternalPlugins)
        {
            // pre-start stuff
            CustomEnvironmentArgs.Apply();
            ConfigBootstrapper.EnsureLatest();

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
            if (isAnyPluginProxying)
            {
                MessageQueueHtppClient client = new MessageQueueHtppClient();
                client.EnsureAvailable();
                client.AddConfig(ConfigKeeper.Instance);
            }
            else
            {
                Console.WriteLine("No plugins running in proxy mode, ignoring MessageQueue status.");
            }

            Core.LoadPlugins();

            // second main part of server start, connect data store, check data state, create data from config, etc
            Core.InitializeData();
        }

        public static void LoadPlugins()
        {
            PluginManager.Initialize();
            PluginManager.WriteCurrentPluginStateToStore();
        }

        public static void InitializeData()
        {
            ConfigurationBuilder.InjectSourceServers();
            ConfigurationBuilder.InjectUsers();

            // build servers should be scafolded last, as they have data that has dependencies on other top level objects
            ConfigurationBuilder.InjectBuildServers();

            IEnumerable<string> orphans = ConfigurationBuilder.FindOrphans();
            foreach(string orphan in orphans)
                Console.WriteLine(orphan);

            if(orphans.Count() > 0)
                throw new ConfigurationException("Orphan records detected. Please merge or delete orphans");
        }
    }
}
