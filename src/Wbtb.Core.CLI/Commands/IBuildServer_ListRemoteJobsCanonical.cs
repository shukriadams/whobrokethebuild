using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IBuildServer_ListRemoteJobsCanonical : ICommand
    {
        public string Describe()
        {
            return @"Lists all jobs on a remote build server. The job identifiers returned can be used for configuration.";
        }

        public void Process(CommandLineSwitches switches) 
        {
            SimpleDI di = new SimpleDI();
            Configuration config = di.Resolve<Configuration>();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();

            if (!switches.Contains("Key"))
            {
                ConsoleHelper.WriteLine($"ERROR : \"Key\" arg required, for buildserver Key to list jobs for", addDate: false);
                Environment.Exit(1);
                return;
            }

            string buildServerKey = switches.Get("Key");
            BuildServer buildServer = config.BuildServers.FirstOrDefault(r => r.Key == buildServerKey);
            if (buildServer == null)
            {
                ConsoleHelper.WriteLine($"ERROR : Buildserver with key \"{buildServerKey}\" not found");
                Environment.Exit(1);
            }

            IBuildServerPlugin buildServerPlugin = pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
            IEnumerable<string> jobs = buildServerPlugin.ListRemoteJobsCanonical(buildServer);
            ConsoleHelper.WriteLine($"Found {jobs.Count()} jobs on buildserver \"{buildServer.Key}\".", addDate: false);

            foreach (string job in jobs)
            {
                ConsoleHelper.WriteLine($"{job}", addDate: false);
            }
        }
    }
}
