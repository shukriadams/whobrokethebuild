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

        private readonly Logger _logger;

        private readonly PluginProvider _pluginProvider;

        private readonly ConsoleCLIHelper _consoleHelper;

        #endregion

        #region CTORS

        public IDataLayer_ResetJob(PluginProvider pluginProvider, ConsoleCLIHelper consoleHelper, Logger logger) 
        {
            _pluginProvider = pluginProvider;
            _consoleHelper = consoleHelper;
            _logger = logger;
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
                _logger.Status($"ERROR : \"--job\" <job> required");
                _consoleHelper.PrintJobs();
                _logger.Status($"use \"--job * \"to wipe all jobs");
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
                    _logger.Status($"ERROR : \"--job\" key {jobKey} does not point to a valid job");
                    _consoleHelper.PrintJobs();
                    Environment.Exit(1);
                    return;
                }
                jobs.Add(job);
            }

            _logger.Status("Performing hard reset");
            _logger.Status($"WARNING - do not reset a job on an actively running server. Stop server, run job, then restart server. Failure to do so can cause concurrency issues in dataset. Rerun this job on a stopped server to reset cleanly.");

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
                        _logger.Status($"Requeued build {build.Key} for processing.");
                    }

                    page++;
                }

                _logger.Status($"Job {job.Name} reset. {reset} records reset.");
            }

        }

        #endregion
    }
}
