﻿using System;
using System.Threading;
using Wbtb.Core.CLI.Lib;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_ResetAllJobs : ICommand
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly ConsoleHelper _consoleHelper;

        #endregion

        #region CTORS

        public IDataLayer_ResetAllJobs(PluginProvider pluginProvider, ConsoleHelper consoleHelper)
        {
            _pluginProvider = pluginProvider;
            _consoleHelper = consoleHelper;
        }

        #endregion

        #region METHODS

        public string Describe()
        {
            return @"Resets all all jobs";
        }

        public void Process(CommandLineSwitches switches)
        {
            bool hard = switches.Contains("hard");

            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            if (hard)
                Console.WriteLine("Performing hard reset");
            else
                Console.WriteLine("Performing reset - to force delete all child records under job use --hard switch");

            Console.Write($"WARNING - do not reset a job on an actively running server. Stop server, run job, then restart server. Failure to do so can cause concurrency issues in dataset. Rerun this job on a stopped server to reset cleanly.");

            foreach (Job job in dataLayer.GetJobs())
            {
                int deleted = dataLayer.ResetJob(job.Id, hard);
                int page = 0;

                while (true)
                {
                    PageableData<Build> builds = dataLayer.PageBuildsByJob(job.Id, page, 100, true);
                    if (builds.Items.Count == 0)
                        break;

                    foreach (Build build in builds.Items)
                    {
                        dataLayer.SaveDaemonTask(new DaemonTask
                        {
                            BuildId = build.Id,
                            Stage = 0, //"BuildEnd",
                            CreatedUtc = DateTime.UtcNow,
                            Src = this.GetType().Name
                        });

                        Thread.Sleep(10);
                        Console.WriteLine($"Requeued build {build.Key} for processing.");
                    }

                    page++;
                }

                Console.Write($"Job {job.Name} reset. {deleted} records deleted.");
            }
        }

        #endregion
    }
}
