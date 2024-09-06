using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Sends alerts when a job is in constant broken status 
    /// </summary>
    public class BuildBrokenRemindDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly SimpleDI _di;

        private readonly ILogger _log;

        private readonly IDaemonTaskController _taskController;

        private readonly PluginProvider _pluginProvider;

        private readonly Cache _cache;

        #endregion

        #region CTORS

        /// Sends alerts when a job is in constant broken status 
        /// </summary>
        public BuildBrokenRemindDaemon(ILogger log, IDaemonTaskController processRunner)
        {
            _log = log;
            _taskController = processRunner;

            _di = new SimpleDI();
            _pluginProvider = _di.Resolve<PluginProvider>();
            _cache = _di.Resolve<Cache>();
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
                    Build latestBuildInJob = dataLayer.GetLatestBuildByJob(job);

                    // check if alert key has been processed
                    if (latestBuildInJob != null && !string.IsNullOrEmpty(latestBuildInJob.IncidentBuildId) && latestBuildInJob.Status == BuildStatus.Failed)
                    {
                        string alertResultsForBuild = string.Empty;

                        foreach (MessageHandler messageHandler in job.Message.Where(r => !string.IsNullOrEmpty(r.Remind)))
                        {
                            int remindInterval = int.Parse(messageHandler.Remind);
                            string alertKey = $"{latestBuildInJob.Id}_{latestBuildInJob.IncidentBuildId}_{job.Key}_remind_{remindInterval}";

                            // test if repeatinterval has elapsed
                            int hoursSinceIncident = (int)Math.Round((DateTime.UtcNow - latestBuildInJob.EndedUtc.Value).TotalHours, 0);
                            int intervalBlock = hoursSinceIncident / remindInterval;
                            if (intervalBlock == 0)
                                // interval not yet elapsed
                                continue;

                            // check if alert for this block has already been sent
                            string intervalKey = $"alert_remind_{latestBuildInJob.IncidentBuildId}_{messageHandler.Plugin}_{messageHandler.User}_{messageHandler.Group}_{intervalBlock}";
                            CachePayload cachedSend = _cache.Get(TypeHelper.Name(this), intervalKey);
                            if (cachedSend.Payload != null)
                                // alread sent
                                continue;

                            IMessagingPlugin messagePlugin = _pluginProvider.GetByKey(messageHandler.Plugin) as IMessagingPlugin;
                            string localResult = messagePlugin.RemindBreaking(messageHandler.User, messageHandler.Group, latestBuildInJob, false);
                            alertResultsForBuild += $"{localResult} for handler {messageHandler.Plugin}, user:{messageHandler.User}|group:{messageHandler.Group}";

                            _cache.Write(TypeHelper.Name(this), intervalKey, string.Empty);

                            dataLayer.SaveStore(new StoreItem
                            {
                                Key = intervalKey,
                                Plugin = TypeHelper.Name(this),
                                Content = $"Date:{DateTime.UtcNow}\n{alertResultsForBuild}"
                            });
                        }

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
