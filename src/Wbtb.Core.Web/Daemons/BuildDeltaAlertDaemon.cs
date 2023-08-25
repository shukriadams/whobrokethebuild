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

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly Cache _cache;

        private readonly BuildEventHandlerHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public BuildDeltaAlertDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            _di = new SimpleDI();
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>();
            _cache = _di.Resolve<Cache>();
            _buildLevelPluginHelper = _di.Resolve<BuildEventHandlerHelper>();
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


                    // alert fails, we alert only latest fail, if for some reason build failed then fixed itself between alert windows 
                    // we ignore those.
                    // note : delta will be null if no build has run or builds have always had same status since job start
                    Build deltaBuild = dataLayer.GetLastJobDelta(job.Id);

                    if (deltaBuild != null && deltaBuild.Status == BuildStatus.Failed)
                    {
                        string failingAlertKey = $"deltaAlert_{job.Key}_{deltaBuild.IncidentBuildId}_{deltaBuild.Status}";
                        string result = string.Empty;

                        // check if delta has already been alerted on
                        if (_cache.Get(TypeHelper.Name(this), failingAlertKey) == null)
                        {
                            if (deltaBuild.Status == BuildStatus.Failed)
                            {
                                _buildLevelPluginHelper.InvokeEvents("OnBroken", job.OnBroken, deltaBuild);

                                foreach (MessageHandler alert in job.Message)
                                {
                                    IMessagingPlugin messagePlugin = _pluginProvider.GetByKey(alert.Plugin) as IMessagingPlugin;
                                    string localResult = messagePlugin.AlertBreaking(alert.User, alert.Group, deltaBuild, false);
                                    result += $"{localResult} for handler {alert.Plugin}, user:{alert.User}|group:{alert.Group}";
                                }
                            }

                            _cache.Write(TypeHelper.Name(this), failingAlertKey, "sent");

                            dataLayer.SaveStore(new StoreItem
                            {
                                Key = failingAlertKey,
                                Plugin = this.GetType().Name,
                                Content = $"Date:{DateTime.UtcNow}\n{result}"
                            });
                        }
                    }

                    // ensure all previous resolve incidents are alerted on,
                    IEnumerable<string> incidentIds = dataLayer.GetIncidentIdsForJob(job, 5);
                    foreach (string incidentId in incidentIds)
                    {
                        Build incident = dataLayer.GetBuildById(incidentId);
                        Build fixingBuild = dataLayer.GetFixForIncident(incident);
                        if (fixingBuild == null)
                            continue;

                        // has fail alert for incident been sent? if not, don't bother alerting fix for it
                        string failingAlertKey = $"deltaAlert_{job.Key}_{incident.IncidentBuildId}_{incident.Status}";
                        if (_cache.Get(TypeHelper.Name(this), failingAlertKey) == null)
                            continue;

                        // has pass alert been sent? if so, don't alert again
                        string passingAlertKey = $"deltaAlert_{job.Key}_{incident.IncidentBuildId}_{fixingBuild.Status}";
                        if (_cache.Get(TypeHelper.Name(this), passingAlertKey) != null)
                            continue;

                        _buildLevelPluginHelper.InvokeEvents("OnFixed", job.OnFixed, fixingBuild);
                        
                        string result = string.Empty;

                        foreach (MessageHandler alert in job.Message)
                        {
                            IMessagingPlugin messagePlugin = _pluginProvider.GetByKey(alert.Plugin) as IMessagingPlugin;
                            string localResult = messagePlugin.AlertPassing(alert.User, alert.Group, incident, fixingBuild);
                            result += $"{localResult} for handler {alert.Plugin}, user:{alert.User}|group:{alert.Group}";
                        }

                        _cache.Write(TypeHelper.Name(this), passingAlertKey, "sent");

                        dataLayer.SaveStore(new StoreItem
                        {
                            Key = passingAlertKey,
                            Plugin = this.GetType().Name,
                            Content = $"Date:{DateTime.UtcNow}\n{result}"
                        });
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError($"Unexpected error on job {job.Name}.", ex);
                }
            }

            
        }
        #endregion
    }
}
