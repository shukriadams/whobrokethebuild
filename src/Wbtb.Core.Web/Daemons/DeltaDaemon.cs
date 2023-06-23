using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    public class DeltaDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly BuildLevelPluginHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;
        #endregion

        #region CTORS

        public DeltaDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            _di = new SimpleDI();
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>();
            _buildLevelPluginHelper = _di.Resolve<BuildLevelPluginHelper>();
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _processRunner.Start(new DaemonWork(this.Work), tickInterval);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _processRunner.Dispose();
        }

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        private void Work()
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.DeltaCalculate.ToString());
            foreach (DaemonTask task in tasks)
            {
                Build build = dataLayer.GetBuildById(task.BuildId);
                Job job = dataLayer.GetJobById(build.JobId);

                // handle current state of game
                Build latestBuild = dataLayer.GetLatestBuildByJob(job);
                Build previousDeltaBuild = dataLayer.GetLastJobDelta(job.Id);

                // no builds for this job yet
                if (latestBuild == null)
                    continue;

                // ignore builds that don't have incidents yet, they need processing by the incident assign daemon first
                if (latestBuild.Status == BuildStatus.Failed && latestBuild.IncidentBuildId == null)
                    continue;

                // this build is first, so it is the first delta
                if (previousDeltaBuild == null)
                {
                    dataLayer.SaveJobDelta(latestBuild);
                }
                else
                {
                    if (latestBuild.Status == BuildStatus.Failed && previousDeltaBuild.Status == BuildStatus.Passed)
                    {
                        // build has gone from passing to failing
                        dataLayer.SaveJobDelta(latestBuild);
                    }
                    else if (latestBuild.Status == BuildStatus.Passed && previousDeltaBuild.Status == BuildStatus.Failed)
                    {
                        // build has gone from failing to passing
                        dataLayer.SaveJobDelta(latestBuild);
                    }
                }

                task.HasPassed = true;
                task.ProcessedUtc = DateTime.UtcNow;
                dataLayer.SaveDaemonTask(task);

                dataLayer.SaveDaemonTask(new DaemonTask { 
                    BuildId = build.Id,
                    Src = this.GetType().Name,
                    Order = 4,
                    TaskKey = DaemonTaskTypes.DeltaChangeAlert.ToString()
                });
            }
        }

        #endregion
    }
}
