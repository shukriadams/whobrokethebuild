using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IBuildServerPlugin_ListRemoteJobsCanonical : ICommand
    {
        public void Process(CommandLineSwitches switches) 
        {
            SimpleDI di = new SimpleDI();
            Config config = di.Resolve<Config>();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();

            if (!switches.Contains("Key"))
            {
                Console.WriteLine($"ERROR : \"Key\" arg required, for buildserver Key to list jobs for");
                Environment.Exit(1);
            }

            string buildServerKey = switches.Get("Key");
            BuildServer buildServer = config.BuildServers.FirstOrDefault(r => r.Key == buildServerKey);
            if (buildServer == null)
            {
                Console.WriteLine($"ERROR : Buildserver with key \"{buildServerKey}\" not found");
                Environment.Exit(1);
            }

            IBuildServerPlugin buildServerPlugin = pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
            IEnumerable<string> jobs = buildServerPlugin.ListRemoteJobsCanonical(buildServer);
            Console.WriteLine($"Found {jobs.Count()} jobs on buildserver \"{buildServer.Key}\".");

            foreach (string job in jobs)
            {
                Console.WriteLine($"{job}");
            }
        }
    }
}
