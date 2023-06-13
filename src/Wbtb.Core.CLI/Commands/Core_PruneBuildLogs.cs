﻿using System;
using System.Collections.Generic;
using System.IO;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class Core_PruneBuildLogs : ICommand
    {
        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataLayerPlugin dataLayer = pluginProvider.GetDistinct<IDataLayerPlugin>();
            Config config = di.Resolve<Config>();
            IEnumerable<string> existingLogFiles = FileSystemHelper.GetFilesUnder(config.BuildLogsDirectory);
            IList<string> knownLogs = new List<string>();
            
            foreach (Job job in dataLayer.GetJobs())
            {
                int i = 0;
                while (true) 
                {
                    PageableData<Build> builds = dataLayer.PageBuildsByJob(job.Id, i , 1000);
                    if (builds.Items.Count == 0)
                        break;

                    foreach (Build build in builds.Items)
                        knownLogs.Add(build.LogPath);

                    i++;
                }
            }

            int removed = 0;

            foreach (string existingLogFile in existingLogFiles) 
            {
                if (knownLogs.Contains(existingLogFile))
                    continue;

                File.Delete(existingLogFile);
                Console.WriteLine($"Removed orphan log {existingLogFile}");
                removed++;
            }

            Console.WriteLine($"Done - deleted {removed} orphan build logs.");
        }
    }
}