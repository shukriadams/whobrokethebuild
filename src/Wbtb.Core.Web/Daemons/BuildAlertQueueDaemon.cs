using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildAlertQueueDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly Logger _log;

        private readonly IDaemonTaskController _taskController;

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public BuildAlertQueueDaemon(Logger log, IDaemonTaskController processRunner)
        {
            _log = log;
            _taskController = processRunner;
            _di = new SimpleDI();
            _pluginProvider = _di.Resolve<PluginProvider>();
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _taskController.WatchForAndRunTasksForDaemon(this, tickInterval);
        }

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        void IWebDaemon.Work()
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            foreach (Job job in dataLayer.GetJobs())
            {
                try
                {
                    // note that alerts don't have their own process task, they run after all other tasks for a job are complete. This was mostly a result of alerting being the opposite of a fixed piece of work, as in,
                    // we do it when all other known pieces of work are done, but as we're always adding new pieces of work in different threads, it's difficult knowing which piece of work has the responsibility of 
                    // sending alert. To be thorough, we might want to us this daemon to create an explicit task, when all other tasks are set, so we can always use task-driven events
                    int unprocessedCount = dataLayer.GetUnprocessedTaskCountForJob(job.Id);
                    if (unprocessedCount > 0)
                    {
                        _log.Debug(this, $"Job {job.Name} still has {unprocessedCount} unprocessed tasks, waiting.");
                        continue;
                    }

                    Build latestBuildInJob = dataLayer.GetLatestBuildByJob(job);

                    // no build in job, assume job hasn't run yet, ignore it
                    if (latestBuildInJob == null)
                    {
                        _log.Debug(this, $"Job {job.Name} has no latest build, assuming not run yet");
                        continue;
                    }

                    // we want to report passes/fails only
                    if (latestBuildInJob.Status != BuildStatus.Failed && latestBuildInJob.Status != BuildStatus.Passed)
                    {
                        _log.Debug(this, $"Latest build {latestBuildInJob.UniquePublicKey} id {latestBuildInJob.Id}, job {job.Name} has status {latestBuildInJob.Status}, only passing/failing jobs processed by this daemon.");
                        continue;
                    }

                    // build is known to be failing, but needs an incidentId assigned first to determine if it
                    // is part of a failure that started on a previous build.
                    if (latestBuildInJob.Status == BuildStatus.Failed && string.IsNullOrEmpty(latestBuildInJob.IncidentBuildId))
                    {
                        _log.Debug(this, $"Latest build {latestBuildInJob.UniquePublicKey} id {latestBuildInJob.Id}, job {job.Name} is failing but has no incidentId assigned yet, waiting");
                        continue;
                    }

                    Build buildToReport = null;

                    if (latestBuildInJob.Status == BuildStatus.Failed)
                        buildToReport = dataLayer.GetBuildById(latestBuildInJob.IncidentBuildId);
                    else 
                        buildToReport = dataLayer.GetLatestPassOrFailBuildByJob(job);

                    // if we already created an alert task for this job, no need to process further
                    IEnumerable<DaemonTask> alertTasksForJob = dataLayer.GetDaemonTasks(buildToReport.Id, ProcessStages.Alert);
                    if (alertTasksForJob.Any()) 
                    {
                        _log.Debug(this, $"Job {job.Name} already has an alert task for it, skipping.");
                        continue;
                    }

                    dataLayer.SaveDaemonTask(new DaemonTask
                    {
                        BuildId = buildToReport.Id,
                        Src = this.GetType().Name,
                        Stage = (int)ProcessStages.Alert
                    });

                    _log.Status(this, $"Queued alert task for build id {latestBuildInJob.Id}, job {job.Name}");
                }
                catch (Exception ex)
                {
                    _log.Error(this, $"Unexpected error on job {job.Name}.", ex);
                }
            }
        }

        DaemonTaskWorkResult IWebDaemon.WorkThreaded(IDataPlugin dataRead, IDataPlugin dataWrite, DaemonTask task, Build build, Job job)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
