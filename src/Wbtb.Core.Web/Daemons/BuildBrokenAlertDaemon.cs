using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildBrokenAlertDaemon : IWebDaemon
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

        private readonly FailingAlertKey _failingAlertKey;

        #endregion

        #region CTORS

        public BuildBrokenAlertDaemon(ILogger log, IDaemonTaskController processRunner, FailingAlertKey failingAlertKey, MutationHelper mutationHelper)
        {
            _log = log;
            _taskController = processRunner;

            _di = new SimpleDI();
            _mutationHelper = mutationHelper;
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>();
            _failingAlertKey = failingAlertKey;
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

                    // no build in job, ignore it
                    if (latestBuildInJob == null)
                        continue;

                    // last build in job is still processing
                    if (string.IsNullOrEmpty(latestBuildInJob.IncidentBuildId))
                        continue;

                    // we want to report on fails only, ignore others
                    if (latestBuildInJob.Status != BuildStatus.Failed)
                        continue;

                    Build incidentBuild = dataLayer.GetBuildById(latestBuildInJob.IncidentBuildId);

                    bool enableMutations = _config.FeatureToggles.Contains("BUILD_MUTATION");
                    bool hasMutated = false;
                    string alertKey = _failingAlertKey.Get(job, incidentBuild);

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
