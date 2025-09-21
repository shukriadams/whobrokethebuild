using System;
using System.Threading;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_ResetAllJobs : ICommand
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly Logger _logger;

        private readonly Cache _cache;

        #endregion

        #region CTORS

        public IDataLayer_ResetAllJobs(PluginProvider pluginProvider, Logger logger, Cache cache)
        {
            _pluginProvider = pluginProvider;
            _logger = logger;
            _cache = cache;

        }

        #endregion

        #region METHODS

        public string Describe()
        {
            return @"Resets all all jobs";
        }

        public void Process(CommandLineSwitches switches)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            _logger.Status("Performing hard reset");

            _logger.Status($"WARNING - do not reset a job on an actively running server. Stop server, run job, then restart server. Failure to do so can cause concurrency issues in dataset. Rerun this job on a stopped server to reset cleanly.");

            bool wipeCache = switches.Contains("cache");
            if (wipeCache)
                _logger.Status(this, "!wiping cached values where possible!");

            foreach (Job job in dataLayer.GetJobs())
            {
                int deleted = dataLayer.ResetJob(job.Id, false);
                int page = 0;

                if (wipeCache)
                {
                    foreach (string logParserKey in job.LogParsers)
                    {
                        IPlugin parserPlugin = _pluginProvider.GetByKey(logParserKey);
                        _cache.Clear(parserPlugin.ContextPluginConfig.Manifest.Key, job);
                    }
                }

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
                            Stage = (int)ProcessStages.BuildEnd,
                            CreatedUtc = DateTime.UtcNow,
                            Src = this.GetType().Name
                        });

                        Thread.Sleep(10);
                        _logger.Status($"Requeued build {build.Key} for processing.");
                    }

                    page++;
                }

                _logger.Status($"Job {job.Name} reset. {deleted} records deleted.");
            }
        }

        #endregion
    }
}
