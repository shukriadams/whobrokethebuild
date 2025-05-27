using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildEndDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonTaskController _taskController;

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        private readonly Configuration _configuration;

        #endregion

        #region CTORS

        public BuildEndDaemon(ILogger log, IDaemonTaskController processRunner)
        {
            _log = log;
            _taskController = processRunner;

            _di = new SimpleDI();
            _pluginProvider = _di.Resolve<PluginProvider>();
            _configuration = _di.Resolve<Configuration>();
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _taskController.WatchForAndRunTasksForDaemon(this, tickInterval, ProcessStages.BuildEnd);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _taskController.Dispose();
        }

        void IWebDaemon.Work()
        {
            throw new NotImplementedException();
        }

        DaemonTaskWorkResult IWebDaemon.WorkThreaded(IDataPlugin dataRead, IDataPlugin dataWrite, DaemonTask task, Build build, Job job) 
        {
            BuildServer buildServer = dataRead.GetBuildServerById(job.BuildServerId);
            IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;

            build = buildServerPlugin.TryUpdateBuild(build);

            // build still not done, continue and wait. Todo : Add forced time out on build here.
            if (!build.EndedUtc.HasValue)
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Blocked, Description = "Build not complete yet" };

            dataWrite.SaveBuild(build);

            // create tasks for next stage

            // after a build is complete we always want to fetch its log
            dataWrite.SaveDaemonTask(new DaemonTask
            {
                BuildId = build.Id,
                Src = this.GetType().Name,
                Stage = (int)ProcessStages.LogImport,
            });

            // We may want to assign revisions + users to a build, this can either be done as below by using information pulled directly from
            // the build server, or from the log. If the latter, then the logImport handler will line up the task for us.
            if (job.LinkRevisions && (!string.IsNullOrEmpty(job.SourceServer) && string.IsNullOrEmpty(job.RevisionAtBuildRegex)))
                dataWrite.SaveDaemonTask(new DaemonTask
                {
                    Stage = (int)ProcessStages.RevisionFromBuildServer,
                    Src = this.GetType().Name,
                    BuildId = build.Id
                });

            if (build.Status == BuildStatus.Failed)
            {
                // these two processors are assigned only on failing builds.
                dataWrite.SaveDaemonTask(new DaemonTask
                {
                    BuildId = build.Id,
                    Src = this.GetType().Name,
                    Stage = (int)ProcessStages.IncidentAssign
                });

                if (job.PostProcessors.Any())
                    dataWrite.SaveDaemonTask(new DaemonTask
                    {
                        BuildId = build.Id,
                        Src = this.GetType().Name,
                        Stage = (int)ProcessStages.PostProcess
                    });
            }
            
            ConsoleHelper.WriteLine(this, $"Build {build.Key} (id:{build.Id}) marked as complete, status is {build.Status}");

            return new DaemonTaskWorkResult();
        }

        #endregion
    }
}
