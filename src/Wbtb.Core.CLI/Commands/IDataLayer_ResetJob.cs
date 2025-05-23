using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_ResetJob : ICommand
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly ConsoleCLIHelper _consoleHelper;

        #endregion

        #region CTORS

        public IDataLayer_ResetJob(PluginProvider pluginProvider, ConsoleCLIHelper consoleHelper) 
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
                ConsoleHelper.WriteLine($"ERROR : \"--job\" <job> required", addDate : false);
                _consoleHelper.PrintJobs();
                ConsoleHelper.WriteLine($"use \"--job * \"to wipe all jobs", addDate: false);
                Environment.Exit(1);
                return;
            }

            string jobKey = switches.Get("job");
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            IList<Job> jobs = new List<Job>();
            if (jobKey == "*")
            {
                jobs = dataLayer.GetJobs().ToList();
            }
            else 
            {
                Job job = dataLayer.GetJobByKey(jobKey);
                if (job == null)
                {
                    ConsoleHelper.WriteLine($"ERROR : \"--job\" key {jobKey} does not point to a valid job", addDate: false);
                    _consoleHelper.PrintJobs();
                    Environment.Exit(1);
                    return;
                }
                jobs.Add(job);
            }

            ConsoleHelper.WriteLine("Performing hard reset", addDate: false);

            ConsoleHelper.WriteLine($"WARNING - do not reset a job on an actively running server. Stop server, run job, then restart server. Failure to do so can cause concurrency issues in dataset. Rerun this job on a stopped server to reset cleanly.", addDate: false);

            bool hard = true;
            foreach (Job job in jobs) 
            {
                int reset = dataLayer.ResetJob(job.Id, hard);
                int page = 0;

                while (true && !hard)
                {
                    PageableData<Build> builds = dataLayer.PageBuildsByJob(job.Id, page, 100, true);
                    if (builds.Items.Count == 0)
                        break;

                    foreach (Build build in builds.Items)
                    {
                        dataLayer.SaveDaemonTask(new DaemonTask
                        {
                            BuildId = build.Id,
                            Stage = (int)ProcessStages.BuildEnd,
                            CreatedUtc = DateTime.UtcNow,
                            Src = this.GetType().Name
                        });

                        Thread.Sleep(10);
                        ConsoleHelper.WriteLine($"Requeued build {build.Key} for processing.", addDate: false);
                    }

                    page++;
                }
                
                ConsoleHelper.WriteLine($"Job {job.Name} reset. {reset} records reset.", addDate: false);
            }

        }

        #endregion
    }
}
