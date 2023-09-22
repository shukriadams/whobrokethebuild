using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Wbtb.Core.CLI.Lib;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_ResetJob : ICommand
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly ConsoleHelper _consoleHelper;

        #endregion

        #region CTORS

        public IDataLayer_ResetJob(PluginProvider pluginProvider, ConsoleHelper consoleHelper) 
        {
            _pluginProvider = pluginProvider;
            _consoleHelper = consoleHelper;
        }

        #endregion

        #region METHODS

        public string Describe()
        {
            return @"Resets all error flags on a job, allowing linking, resolving, build log processing to run on all builds in that job. Alerts are not resent.";
        }

        public void Process(CommandLineSwitches switches) 
        {
            if (!switches.Contains("job"))
            {
                Console.WriteLine($"ERROR : \"--job\" <job> required");
                _consoleHelper.PrintJobs();
                Console.WriteLine($"use \"--job * \"to wipe all jobs");
                Environment.Exit(1);
                return;
            }

            string jobid = switches.Get("job");
            bool hard = switches.Contains("hard");
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            IList<Job> jobs = new List<Job>();
            if (jobid == "*")
            {
                jobs = dataLayer.GetJobs().ToList();
            }
            else 
            {
                Job job = dataLayer.GetJobById(jobid);
                if (job == null)
                {
                    Console.WriteLine($"ERROR : \"--job\" id {jobid} does not point to a valid job");
                    _consoleHelper.PrintJobs();
                    Environment.Exit(1);
                    return;
                }
                jobs.Add(job);
            }


            if (hard)
                Console.WriteLine("Performing hard reset");
            else
                Console.WriteLine("Performing reset - to force delete all child records under job use --hard switch");

            Console.Write($"WARNING - do not reset a job on an actively running server. Stop server, run job, then restart server. Failure to do so can cause concurrency issues in dataset. Rerun this job on a stopped server to reset cleanly.");

            foreach (Job job in jobs) 
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
