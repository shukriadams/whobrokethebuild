using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    public class BuildEndDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly BuildLevelPluginHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;
        #endregion

        #region CTORS

        public BuildEndDaemon(ILogger log, IDaemonProcessRunner processRunner)
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
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.BuildEnd.ToString());

            foreach(DaemonTask task in tasks)
            {
                Build build = dataLayer.GetBuildById(task.BuildId);
                Job job = dataLayer.GetJobById(build.JobId);
                BuildServer buildserver = dataLayer.GetBuildServerById(job.BuildServerId);
                IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildserver.Plugin) as IBuildServerPlugin;

                // build is already marked as done, this should not happen
                if (build.EndedUtc != null) 
                { 
                    task.ProcessedUtc = build.EndedUtc;
                    task.HasPassed = true;
                    task.Result = "Already finished";
                    dataLayer.SaveDaemonTask(task);
                    continue;
                }

                build = buildServerPlugin.TryUpdateBuild(build);

                // build still not done
                if (!build.EndedUtc.HasValue)
                    continue;

                dataLayer.SaveBuild(build);

                task.HasPassed = true;
                task.ProcessedUtc = DateTime.UtcNow;
                dataLayer.SaveDaemonTask(task);

                // create tasks for next stage
                dataLayer.SaveDaemonTask(new DaemonTask
                {
                    BuildId = build.Id,
                    Src = this.GetType().Name,
                    Order = 1,
                    TaskKey = DaemonTaskTypes.LogImport.ToString()
                });

                dataLayer.SaveDaemonTask(new DaemonTask
                {
                    BuildId = build.Id,
                    Src = this.GetType().Name,
                    Order = 4,
                    TaskKey = DaemonTaskTypes.DeltaCalculate.ToString()
                });

                if (build.Status == BuildStatus.Failed) 
                    dataLayer.SaveDaemonTask(new DaemonTask { 
                        BuildId = build.Id,
                        Src = this.GetType().Name,
                        Order = 1,
                        TaskKey = DaemonTaskTypes.IncidentAssign.ToString()
                    });
            }
        }

        #endregion
    }
}
