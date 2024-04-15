using System;
using System.Collections.Generic;
using System.IO;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class Core_PruneBuildLogs : ICommand
    {
        public string Describe()
        {
            return @"Deletes build logs that are no longer associated with a build.";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetDistinct<IDataPlugin>();
            Configuration config = di.Resolve<Configuration>();
            IEnumerable<string> existingLogFiles = FileSystemHelper.GetFilesUnder(config.BuildLogsDirectory);
            IList<string> knownLogs = new List<string>();
            
            foreach (Job job in dataLayer.GetJobs())
            {
                int i = 0;
                while (true) 
                {
                    PageableData<Build> builds = dataLayer.PageBuildsByJob(job.Id, i , 1000, false);
                    if (builds.Items.Count == 0)
                        break;

                    foreach (Build build in builds.Items)
                        knownLogs.Add(Build.GetLogPath(config, job, build));

                    i++;
                }
            }

            int removed = 0;

            foreach (string existingLogFile in existingLogFiles) 
            {
                if (knownLogs.Contains(existingLogFile))
                    continue;

                File.Delete(existingLogFile);
                ConsoleHelper.WriteLine($"Removed orphan log {existingLogFile}");
                removed++;
            }

            ConsoleHelper.WriteLine($"Done - deleted {removed} orphan build logs.");
        }
    }
}
