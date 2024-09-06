using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Runs import build and import log on build systems.
    /// </summary>
    public class BuildDeltaAlertDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonTaskController _taskController;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly Cache _cache;

        private readonly BuildEventHandlerHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;

        private readonly MutationHelper _mutationHelper;

        #endregion

        #region CTORS

        public BuildDeltaAlertDaemon(ILogger log, IDaemonTaskController processRunner, MutationHelper mutationHelper)
        {
            _log = log;
            _taskController = processRunner;

            _di = new SimpleDI();
            _mutationHelper = mutationHelper;
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>();
            _cache = _di.Resolve<Cache>();
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

        private string FailingAlertKey(Job job, Build incident)
        {
            string incidentMutation = _mutationHelper.GetBuildMutation(incident);
            return $"{incidentMutation}_{job.Key}_deltaAlert_{incident.Status}";
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
                    // scan on implausably high taskgroup so we assume we always see other tasks for this job
                    IEnumerable<DaemonTask> blockingTasksForJob = dataLayer.DaemonTasksBlockedForJob(job.Id, 99999);
                    if (blockingTasksForJob.Any())
                    {
                        _log.LogDebug($"Job {job.Name} blocked by {blockingTasksForJob.Count()} tasks, waiting.");
                        continue;
                    }

                    Build latestBuildInJob = dataLayer.GetLatestBuildByJob(job);

                    if (latestBuildInJob != null && latestBuildInJob.Status == BuildStatus.Failed && !string.IsNullOrEmpty(latestBuildInJob.IncidentBuildId))
                    {
                        Build incidentBuild = dataLayer.GetBuildById(latestBuildInJob.IncidentBuildId);

                        bool enableMutations = _config.FeatureToggles.Contains("BUILD_MUTATION");
                        bool hasMutated = false;
                        string alertKey = FailingAlertKey(job, incidentBuild);

                        if (enableMutations)
                        {
                            string currentBuildMutation = _mutationHelper.GetBuildMutation(latestBuildInJob);
        
                            // get mutation of preceding breaking build (in same incident)
                            Build previousBreakingBuild = dataLayer.GetPrecedingBuildInIncident(latestBuildInJob);
                            string previousBuildMutation = null;
                            if (previousBreakingBuild != null)
                                previousBuildMutation = _mutationHelper.GetBuildMutation(previousBreakingBuild);

                            // build has mutated of current mutation is different from previous one
                            hasMutated = previousBuildMutation != null && previousBuildMutation != currentBuildMutation;
                        }

                        if (_cache.Get(TypeHelper.Name(this), job, incidentBuild, alertKey).Payload == null) 
                        {
                            string result = string.Empty;

                            _buildLevelPluginHelper.InvokeEvents("OnBroken", job.OnBroken, latestBuildInJob);

                            // filter out remind handlers, we don't want to send regular alerts to them
                            foreach (MessageHandler messageHandler in job.Message.Where(handler => string.IsNullOrEmpty(handler.Remind)))
                            {
                                IMessagingPlugin messagePlugin = _pluginProvider.GetByKey(messageHandler.Plugin) as IMessagingPlugin;
                                string localResult = messagePlugin.AlertBreaking(messageHandler.User, messageHandler.Group, latestBuildInJob, hasMutated, false);
                                result += $"{localResult} for handler {messageHandler.Plugin}, user:{messageHandler.User}|group:{messageHandler.Group}";
                            }

                            _cache.Write(TypeHelper.Name(this), job, incidentBuild, alertKey, "sent");

                            dataLayer.SaveStore(new StoreItem
                            {
                                Key = alertKey,
                                Plugin = this.GetType().Name,
                                Content = $"Date:{DateTime.UtcNow}\n{result}"
                            });

                            ConsoleHelper.WriteLine(this, $"Alerted job {job.Name} broken by build {incidentBuild.Key} (id:{incidentBuild.Id})");
                        }
                    }


                    // todo : this block could be decoupled from the preceeding fail alert block, they are functionally unrelated
                    // ensure all previous resolve incidents are alerted on,
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
                        string failingAlertKey = FailingAlertKey(job, incident);
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
