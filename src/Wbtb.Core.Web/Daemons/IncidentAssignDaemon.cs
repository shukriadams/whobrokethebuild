using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Sets incidentbuild on build records, this ties builds together when an incident occurs
    /// </summary>
    public class IncidentAssignDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        public static int TaskGroup = 1;

        #endregion

        #region CTORS

        public IncidentAssignDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            SimpleDI di = new SimpleDI();
            _config = di.Resolve<Configuration>();
            _pluginProvider = di.Resolve<PluginProvider>(); 

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
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.IncidentAssign.ToString());

            foreach (DaemonTask task in tasks)
            {
                Build build = dataLayer.GetBuildById(task.BuildId);
                if (dataLayer.DaemonTasksBlocked(build.Id, TaskGroup))
                    continue;

                Build previousBuild = dataLayer.GetPreviousBuild(build);
                string description = string.Empty;
                if (previousBuild == null || previousBuild.Status == BuildStatus.Passed)
                {
                    // this build is either the very first build in job and has failed (way to start!) or is the first build of sequence to fail,
                    // mark it as the incident build
                    build.IncidentBuildId = build.Id;
                    dataLayer.SaveBuild(build);

                    task.HasPassed = true;
                    task.ProcessedUtc = DateTime.UtcNow;
                    dataLayer.SaveDaemonTask(task);

                    continue;
                }

                // build is part of a continuing incident
                if (previousBuild != null && !string.IsNullOrEmpty(previousBuild.IncidentBuildId))
                {
                    build.IncidentBuildId = previousBuild.IncidentBuildId;
                    dataLayer.SaveBuild(build);

                    task.HasPassed = true;
                    task.ProcessedUtc = DateTime.UtcNow;
                    dataLayer.SaveDaemonTask(task);

                    continue;
                }

                // if reach here, incidentbuild could not be set, create a buildflag record that prevents build from being re-processed
                task.HasPassed = false;
                task.ProcessedUtc = DateTime.UtcNow;
                task.Result = "Failed to assign incident";
                dataLayer.SaveDaemonTask(task);
            }
        }

        #endregion
    }
}
