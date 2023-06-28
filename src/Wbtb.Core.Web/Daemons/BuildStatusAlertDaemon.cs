using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Runs import build and import log on build systems.
    /// </summary>
    public class BuildStatusAlertDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly BuildLevelPluginHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;

        public static int TaskGroup = 6;

        #endregion

        #region CTORS

        public BuildStatusAlertDaemon(ILogger log, IDaemonProcessRunner processRunner)
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
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.DeltaChangeAlert.ToString());
            DaemonActiveProcesses activeItems = _di.Resolve<DaemonActiveProcesses>();

            try
            {
                foreach (DaemonTask task in tasks)
                {
                    try
                    {
                        Build build = dataLayer.GetBuildById(task.BuildId);

                        activeItems.Add(this, $"Task : {task.Id}, Build {build.Id}");

                        if (dataLayer.DaemonTasksBlocked(build.Id, TaskGroup))
                            continue;

                        Job job = dataLayer.GetJobById(build.JobId);

                        // handle current state of game
                        Build deltaBuild = dataLayer.GetLastJobDelta(job.Id);

                        // if delta not yet calculated, ignore alerts for this job
                        if (deltaBuild == null)
                        {
                            task.Result = $"Delta not found job {job.Name}";
                            task.HasPassed = false;
                            task.ProcessedUtc = DateTime.UtcNow;
                            dataLayer.SaveDaemonTask(task);
                            continue;
                        }

                        // check if delta has already been alerted on
                        string deltaAlertKey = $"deltaAlert_{deltaBuild.IncidentBuildId}_{deltaBuild.Status}";
                        StoreItem deltaAlerted = dataLayer.GetStoreItemByKey(deltaAlertKey);
                        if (deltaAlerted != null)
                        {
                            task.Result = $"Delta key {deltaAlertKey} has already been sent";
                            task.HasPassed = true;
                            task.ProcessedUtc = DateTime.UtcNow;
                            dataLayer.SaveDaemonTask(task);
                            continue;
                        }

                        if (deltaBuild.Status == BuildStatus.Failed)
                        {
                            // build has gone from passing to failing
                            _buildLevelPluginHelper.InvokeEvents("OnBroken", job.OnBroken, deltaBuild);

                            foreach (MessageHandler alert in job.Message)
                            {
                                IMessagingPlugin messagePlugin = _pluginProvider.GetByKey(alert.Plugin) as IMessagingPlugin;
                                messagePlugin.AlertBreaking(alert, deltaBuild);
                            }

                        }
                        else if (deltaBuild.Status == BuildStatus.Passed)
                        {
                            // build has gone from failing to passing
                            _buildLevelPluginHelper.InvokeEvents("OnFixed", job.OnFixed, deltaBuild);

                            string lastbreakingId = dataLayer.GetIncidentIdsForJob(job).FirstOrDefault();
                            Build lastBreakingBuild = null;

                            if (!string.IsNullOrEmpty(lastbreakingId))
                                lastBreakingBuild = dataLayer.GetBuildById(lastbreakingId);

                            // ugly cludge 
                            if (lastBreakingBuild == null)
                                lastBreakingBuild = deltaBuild;

                            foreach (MessageHandler alert in job.Message)
                            {
                                IMessagingPlugin messagePlugin = _pluginProvider.GetByKey(alert.Plugin) as IMessagingPlugin;
                                messagePlugin.AlertPassing(alert, lastBreakingBuild, deltaBuild);
                            }
                        }

                        dataLayer.SaveStore(new StoreItem
                        {
                            Key = deltaAlertKey,
                            Plugin = this.GetType().Name
                        });

                        task.HasPassed = true;
                        task.ProcessedUtc = DateTime.UtcNow;
                        dataLayer.SaveDaemonTask(task);
                    }
                    catch (Exception ex)
                    {
                        task.HasPassed = false;
                        task.Result = ex.ToString();
                        task.ProcessedUtc = DateTime.UtcNow;
                        dataLayer.SaveDaemonTask(task);
                    }
                }
            }
            finally
            {
                activeItems.Clear(this);
            }

            
        }
        #endregion
    }
}
