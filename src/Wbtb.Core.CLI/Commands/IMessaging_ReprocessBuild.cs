using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IMessaging_ReprocessBuild : ICommand
    {
        public string Describe()
        {
            return @"Reprocesses message alerts for a given build. Requires that build already has an alert process associated with it. This exists only if the build was the last in the job in a lull window.";
        }

        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("build"))
            {
                ConsoleHelper.WriteLine($"ERROR : \"build\" <unique public id> required", addDate: false);
                Environment.Exit(1);
                return;
            }

            string buildId = switches.Get("build");
            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();

            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Build build = dataLayer.GetBuildByUniquePublicIdentifier(buildId);
            if (build == null) 
            {
                ConsoleHelper.WriteLine($"ERROR : \"--build\" UPID {buildId} does not point to a valid build", addDate: false);
                Environment.Exit(1);
            }

            IEnumerable<DaemonTask> daemonTasks = dataLayer.GetDaemonTasksByBuild(build.Id).Where(dt => dt.Stage == (int)ProcessStages.Alert);
            if (!daemonTasks.Any()) 
            {
                ConsoleHelper.WriteLine($"No alert tasks found for build UPID {buildId}", addDate: false);
                Environment.Exit(1);
            }

            foreach (DaemonTask daemonTask in daemonTasks) 
            {
                daemonTask.AppendResult($"Queued for reprocess at {DateTime.UtcNow.ToHumanString()}");
                daemonTask.ProcessedUtc = null;
                dataLayer.SaveDaemonTask(daemonTask);
                ConsoleHelper.WriteLine($"Requeued alert on daemontask {daemonTask.Id}", addDate: false);
            }

            ConsoleHelper.WriteLine($"Finished resetting {daemonTasks.Count()} processes", addDate: false);
        }
    }
}
