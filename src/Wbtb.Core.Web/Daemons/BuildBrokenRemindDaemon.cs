﻿using Serilog;
using System;
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

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly Cache _cache;

        #endregion

        #region CTORS

        /// Sends alerts when a job is in constant broken status 
        /// </summary>
        public BuildBrokenRemindDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            _di = new SimpleDI();
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>();
            _cache = _di.Resolve<Cache>();
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
                    Build latestDeltaBuild = dataLayer.GetLastJobDelta(job.Id);
                    string alertKey = "";

                    // check if alert key has been processed
                    if (latestDeltaBuild != null && !string.IsNullOrEmpty(latestDeltaBuild.IncidentBuildId) && latestDeltaBuild.Status == BuildStatus.Failed)
                    {
                        string alertResultsForBuild = string.Empty;

                        foreach (MessageHandler alert in job.Message)
                        {
                            int remindInterval = int.Parse(alert.Remind);

                            // test if repeatinterval has elapsed
                            int hoursSinceIncident = (int)Math.Round((DateTime.UtcNow - latestDeltaBuild.EndedUtc.Value).TotalHours, 0);
                            int intervalBlock = hoursSinceIncident / remindInterval;
                            if (intervalBlock == 0)
                                // interval not yet elapsed
                                continue;

                            // check if alert for this block has already been sent
                            string intervalKey = $"alert_remind_{latestDeltaBuild.UniquePublicKey}_{alert.Plugin}_{alert.User}_{alert.Group}_{intervalBlock}";
                            CachePayload cachedSend = _cache.Get(this.GetType().Name, intervalKey);
                            if (cachedSend != null)
                                // alread sent
                                continue;

                            IMessagingPlugin messagePlugin = _pluginProvider.GetByKey(alert.Plugin) as IMessagingPlugin;
                            string localResult = messagePlugin.RemindBreaking(alert.User, alert.Group, latestDeltaBuild, false);
                            alertResultsForBuild += $"{localResult} for handler {alert.Plugin}, user:{alert.User}|group:{alert.Group}";

                            _cache.Write(this.GetType().Name, intervalKey, string.Empty);
                        }

                        dataLayer.SaveStore(new StoreItem
                        {
                            Key = alertKey,
                            Plugin = this.GetType().Name,
                            Content = $"Date:{DateTime.UtcNow}\n{alertResultsForBuild}"
                        });
                    }
                }
                catch (Exception ex)
                {  
                    _log.Error($"Unexpected error on job {job.Name}.", ex);
                }
            }
        }

        #endregion
    }
}
