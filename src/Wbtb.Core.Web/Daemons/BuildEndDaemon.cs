using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly BuildEventHandlerHelper _buildLevelPluginHelper;

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
            DaemonActiveProcesses activeItems = _di.Resolve<DaemonActiveProcesses>();
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.BuildEnd.ToString());

            try
            {
                foreach (DaemonTask task in tasks)
                {
                    try
                    {
                        Build build = dataLayer.GetBuildById(task.BuildId);
                        Job job = dataLayer.GetJobById(build.JobId);
                        BuildServer buildserver = dataLayer.GetBuildServerById(job.BuildServerId);
                        IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildserver.Plugin) as IBuildServerPlugin;

                        activeItems.Add(this, $"Task : {task.Id}, Build {build.Id}");

                        build = buildServerPlugin.TryUpdateBuild(build);

                        // build still not done, contine and wait. Todo : Add forced time out on build here.
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

                        if (!string.IsNullOrEmpty(job.SourceServer) && string.IsNullOrEmpty(job.RevisionAtBuildRegex))
                            dataLayer.SaveDaemonTask(new DaemonTask
                            {
                                TaskKey = DaemonTaskTypes.AddBuildRevisionsFromBuildServer.ToString(),
                                Order = 0,
                                Src = this.GetType().Name,
                                BuildId = build.Id
                            });

                        dataLayer.SaveDaemonTask(new DaemonTask
                        {
                            BuildId = build.Id,
                            Src = this.GetType().Name,
                            Order = 5,
                            TaskKey = DaemonTaskTypes.DeltaChangeAlert.ToString()
                        });

                        if (build.Status == BuildStatus.Failed)
                        {
                            dataLayer.SaveDaemonTask(new DaemonTask
                            {
                                BuildId = build.Id,
                                Src = this.GetType().Name,
                                Order = 1,
                                TaskKey = DaemonTaskTypes.IncidentAssign.ToString()
                            });

                            if (job.PostProcessors.Any())
                                dataLayer.SaveDaemonTask(new DaemonTask
                                {
                                    BuildId = build.Id,
                                    Src = this.GetType().Name,
                                    Order = 4,
                                    TaskKey = DaemonTaskTypes.PostProcess.ToString()
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = false;
                        task.Result = ex.ToString();
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
