using System;
using System.Collections.Generic;
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

        private readonly Logger _log;

        private readonly IDaemonTaskController _taskController;

        private readonly PluginProvider _pluginProvider;

        private readonly Cache _cache;

        #endregion

        #region CTORS

        /// Sends alerts when a job is in constant broken status 
        /// </summary>
        public BuildBrokenRemindDaemon(Logger log, IDaemonTaskController processRunner)
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

                    // no latest build means job hasn't run yet, ignore it
                    if (latestBuildInJob == null) 
                    {
                        _log.Debug(this, $"No latest build in job {job.Name}", 4);
                        continue;
                    }

                    // if latest build isn't failing, no need to remind on it
                    if (latestBuildInJob.Status != BuildStatus.Failed)
                    {
                        _log.Debug(this, $"Job {job.Name} status is {latestBuildInJob.Status}, only failing needs reminding", 4);
                        continue;
                    }

                    // if incident id not set yet, build is still being processed. ignore for now
                    if (string.IsNullOrEmpty(latestBuildInJob.IncidentBuildId))
                    {
                        _log.Debug(this, $"latest build in {job.Name} is upk {latestBuildInJob.UniquePublicKey}, has no incident id yet.", 4);
                        continue;
                    }

                    // check if alert key has been processed
                    string alertResultsForBuild = string.Empty;

                    IEnumerable<MessageHandler> remindMessages = job.Message.Where(r => !string.IsNullOrEmpty(r.Remind));
                    if (!remindMessages.Any()) 
                        _log.Debug(this, $"No remind messages defined for {job.Name}.", 4);

                    foreach (MessageHandler messageHandler in remindMessages)
                    {
                        int remindInterval = int.Parse(messageHandler.Remind);
                        string alertKey = $"{latestBuildInJob.Id}_{latestBuildInJob.IncidentBuildId}_{job.Key}_remind_{remindInterval}";

                        // test if repeatinterval has elapsed
                        int hoursSinceIncident = (int)Math.Round((DateTime.UtcNow - latestBuildInJob.EndedUtc.Value).TotalHours, 0);
                        int intervalBlock = hoursSinceIncident / remindInterval;
                        if (intervalBlock == 0) 
                        {
                            _log.Debug(this, $"interval block for job {job.Name} is zero. hours since last incident {hoursSinceIncident}, interval is {remindInterval}", 4);
                            // interval not yet elapsed
                            continue;
                        }

                        // check if alert for this block has already been sent
                        string intervalKey = $"alert_remind_{latestBuildInJob.IncidentBuildId}_{messageHandler.Plugin}_{messageHandler.User}_{messageHandler.Group}_{intervalBlock}";
                        CachePayload cachedSend = _cache.Get(TypeHelper.Name(this), intervalKey);
                        if (cachedSend.Payload != null)
                        {
                            // already sent
                            _log.Debug(this, $"reminder for job {job.Name} has already been sent", 4);
                            continue;
                        }

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

                        _log.Debug(this, $"reminder for job {job.Name} has been sent", 4);

                    }
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
