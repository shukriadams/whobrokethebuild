using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildFixedAlertDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonTaskController _taskController;

        private readonly PluginProvider _pluginProvider;

        private readonly Cache _cache;

        private readonly BuildEventHandlerHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;

        private readonly MutationHelper _mutationHelper;

        private readonly FailingAlertKey _failingAlertKey;

        #endregion

        #region CTORS

        public BuildFixedAlertDaemon(ILogger log, IDaemonTaskController processRunner, FailingAlertKey failingAlertKey, MutationHelper mutationHelper)
        {
            _log = log;
            _taskController = processRunner;

            _di = new SimpleDI();
            _mutationHelper = mutationHelper;
            _pluginProvider = _di.Resolve<PluginProvider>();
            _cache = _di.Resolve<Cache>();
            _failingAlertKey = failingAlertKey;
            _buildLevelPluginHelper = _di.Resolve<BuildEventHandlerHelper>();
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _taskController.WatchForAndRunTasksForDaemon(this, tickInterval);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _taskController.Dispose();
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
                    // scan on implausibly high task group so we assume we always see other tasks for this job
                    IEnumerable<DaemonTask> blockingTasksForJob = dataLayer.DaemonTasksBlockedForJob(job.Id, 99999);
                    if (blockingTasksForJob.Any())
                    {
                        _log.LogDebug($"Job {job.Name} blocked by {blockingTasksForJob.Count()} tasks, waiting.");
                        continue;
                    }

                    Build latestBuildInJob = dataLayer.GetLatestBuildByJob(job);

                    // ensure all previous resolve incidents are alerted on, we look back 5 to make sure we cover 
                    // recent builds, 1 should be enough but overcovering better.
                    // todo : find a way to look up incidents that haven't yet been alerted on
                    IEnumerable<string> incidentIds = dataLayer.GetIncidentIdsForJob(job, 5);
                    foreach (string incidentId in incidentIds)
                    {
                        Build incident = dataLayer.GetBuildById(incidentId);
                        Build fixingBuild = dataLayer.GetFixForIncident(incident);
                        if (fixingBuild == null)
                            continue;

                        // has pass alert been sent? if so, don't alert again
                        string passingAlertKey = $"{incident.Key}_{job.Key}_deltaAlert_{fixingBuild.Status}";
                        if (_cache.Get(TypeHelper.Name(this), job, incident, passingAlertKey).Payload != null)
                            continue;

                        string incidentMutation = _mutationHelper.GetBuildMutation(incident);

                        // has fail alert for incident been sent? if not, don't bother alerting fix for it
                        string failingAlertKey = _failingAlertKey.Get(job, incident);
                        if (_cache.Get(TypeHelper.Name(this), job, incident, failingAlertKey).Payload == null)
                            continue;

                        _buildLevelPluginHelper.InvokeEvents("OnFixed", job.OnFixed, fixingBuild);

                        string result = string.Empty;

                        foreach (MessageHandler alert in job.Message)
                        {
                            IMessagingPlugin messagePlugin = _pluginProvider.GetByKey(alert.Plugin) as IMessagingPlugin;
                            string localResult = messagePlugin.AlertPassing(alert.User, alert.Group, incident, fixingBuild);
                            result += $"{localResult} for handler {alert.Plugin}, user:{alert.User}|group:{alert.Group}";
                        }

                        _cache.Write(TypeHelper.Name(this), job, incident, passingAlertKey, "sent");

                        dataLayer.SaveStore(new StoreItem
                        {
                            Key = passingAlertKey,
                            Plugin = this.GetType().Name,
                            Content = $"Date:{DateTime.UtcNow}\n{result}"
                        });

                        ConsoleHelper.WriteLine(this, $"Alerted job {job.Name} passing at incident {incident.Key} (id:{incident.Id})");
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError($"Unexpected error on job {job.Name}.", ex);
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
