using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildAlertDaemon : IWebDaemon
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

        public BuildAlertDaemon(ILogger log, IDaemonTaskController processRunner, FailingAlertKey failingAlertKey, MutationHelper mutationHelper)
        {
            _log = log;
            _taskController = processRunner;

            _di = new SimpleDI();
            _mutationHelper = mutationHelper;
            _pluginProvider = _di.Resolve<PluginProvider>();
            _failingAlertKey = failingAlertKey;
            _cache = _di.Resolve<Cache>();
            _buildLevelPluginHelper = _di.Resolve<BuildEventHandlerHelper>();
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _taskController.WatchForAndRunTasksForDaemon(this, tickInterval, ProcessStages.Alert);
        }

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        void IWebDaemon.Work()
        {
            throw new NotImplementedException();
        }

        DaemonTaskWorkResult IWebDaemon.WorkThreaded(IDataPlugin dataRead, IDataPlugin dataWrite, DaemonTask task, Build build, Job job)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            try
            {
                string result = string.Empty;

                if (build.Status == BuildStatus.Failed) 
                {
                    string alertKey = _failingAlertKey.Get(job, build);
                    if (_cache.Get(TypeHelper.Name(this), job, build, alertKey).Payload != null)
                    {
                        _log.LogTrace($"Build {build.UniquePublicKey} id {build.Id}, job {job.Name} has already been alerted as broken, skipping");
                        return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Passed, Description = $"bypassed from cache, plugin \"{TypeHelper.Name(this)}\", job \"{job.Key}\", build \"{build.UniquePublicKey}\", key \"{alertKey}\"." };
                    }

                    _buildLevelPluginHelper.InvokeEvents("OnBroken", job.OnBroken, build);

                    // filter out remind handlers, we don't want to send regular alerts to them
                    foreach (MessageHandler messageHandler in job.Message.Where(handler => string.IsNullOrEmpty(handler.Remind)))
                    {
                        IMessagingPlugin messagePlugin = _pluginProvider.GetByKey(messageHandler.Plugin) as IMessagingPlugin;
                        string localResult = messagePlugin.AlertBreaking(messageHandler.User, messageHandler.Group, build, false, false);
                        result += $"{localResult} for handler {messageHandler.Plugin}, user:{messageHandler.User}|group:{messageHandler.Group}\n";
                        _log.LogTrace($"Processed broken message {messagePlugin.ContextPluginConfig.Key}, incident {build.IncidentBuildId}, job {job.Name}, result was {localResult}");
                    }

                    dataLayer.SaveStore(new StoreItem
                    {
                        Key = alertKey,
                        Plugin = this.GetType().Name,
                        Content = $"Date:{DateTime.UtcNow}\n{result}"
                    });

                    _cache.Write(TypeHelper.Name(this), job, build, alertKey, "sent");
                    ConsoleHelper.WriteLine(this, $"Alerted job {job.Name} broken by build {build.Key} (id:{build.Id})");
                }

                if (build.Status == BuildStatus.Passed)
                {
                    Build incident = dataLayer.GetLastIncidentBefore(build);
                    if (incident == null)
                    {
                        _log.LogTrace($"Build {build.UniquePublicKey} id {build.Id}, job {job.Name} is passed, but no incident found, pass alert no needed.");
                        return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Passed, Description = $"no incident for pass, ignoring." };
                    }

                    // has pass alert been sent? if so, don't alert again
                    string alertKey = $"{incident.Key}_{job.Key}_deltaAlert_{build.Status}";
                    if (_cache.Get(TypeHelper.Name(this), job, incident, alertKey).Payload != null)
                    {
                        _log.LogTrace($"Build {build.UniquePublicKey} id {build.Id}, job {job.Name} has already been alerted as fixed, skipping");
                        return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Passed, Description = $"bypassed from cache, plugin \"{TypeHelper.Name(this)}\", job \"{job.Key}\", build \"{build.UniquePublicKey}\", key \"{alertKey}\"." };
                    }

                    _buildLevelPluginHelper.InvokeEvents("OnFixed", job.OnFixed, build);
                    foreach (MessageHandler alert in job.Message)
                    {
                        IMessagingPlugin messagePlugin = _pluginProvider.GetByKey(alert.Plugin) as IMessagingPlugin;
                        string localResult = messagePlugin.AlertPassing(alert.User, alert.Group, incident, build);
                        result += $"{localResult} for handler {alert.Plugin}, user:{alert.User}|group:{alert.Group}\n";
                        _log.LogTrace($"Processed fixed message {messagePlugin.ContextPluginConfig.Key}, incident {incident.IncidentBuildId}, job {job.Name}, result was {localResult}");
                    }

                    ConsoleHelper.WriteLine(this, $"Alerted job {job.Name} passing at build {build.Key} (id:{build.Id}), incident was {incident.Key} (id:{incident.Id})");
                }

                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Passed, Description = result };

            }
            catch (Exception ex)
            {
                _log.LogError($"Unexpected error on job {job.Name}.", ex);
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Failed, Description = ex.ToString() };
            }
        }

        #endregion

    }
}
