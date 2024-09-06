using Microsoft.Extensions.Logging;
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
            _taskController.WatchForAndRunTasksForDaemon(new DaemonWorkThreaded(this.WorkThreaded), tickInterval, this, DaemonTaskTypes.BuildEnd);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _taskController.Dispose();
        }

        private DaemonTaskWorkResult WorkThreaded(IDataPlugin dataRead, IDataPlugin dataWrite, DaemonTask task, Build build, Job job) 
        {
            BuildServer buildserver = dataRead.GetBuildServerById(job.BuildServerId);
            IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildserver.Plugin) as IBuildServerPlugin;

            build = buildServerPlugin.TryUpdateBuild(build);

            // build still not done, contine and wait. Todo : Add forced time out on build here.
            if (!build.EndedUtc.HasValue)
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Blocked, Description = "Build not complete yet" };

            dataWrite.SaveBuild(build);

            // create tasks for next stage
            dataWrite.SaveDaemonTask(new DaemonTask
            {
                BuildId = build.Id,
                Src = this.GetType().Name,
                Stage = (int)DaemonTaskTypes.LogImport,
            });

            if (!string.IsNullOrEmpty(job.SourceServer) && string.IsNullOrEmpty(job.RevisionAtBuildRegex))
                dataWrite.SaveDaemonTask(new DaemonTask
                {
                    Stage = (int)DaemonTaskTypes.RevisionFromBuildServer,
                    Src = this.GetType().Name,
                    BuildId = build.Id
                });

            if (build.Status == BuildStatus.Failed)
            {
                dataWrite.SaveDaemonTask(new DaemonTask
                {
                    BuildId = build.Id,
                    Src = this.GetType().Name,
                    Stage = (int)DaemonTaskTypes.IncidentAssign
                });

                if (job.PostProcessors.Any())
                    dataWrite.SaveDaemonTask(new DaemonTask
                    {
                        BuildId = build.Id,
                        Src = this.GetType().Name,
                        Stage = (int)DaemonTaskTypes.PostProcess
                    });
            }
            
            ConsoleHelper.WriteLine(this, $"Build {build.Key} (id:{build.Id}) marked as complete, status is {build.Status}");

            return new DaemonTaskWorkResult();
        }

        #endregion
    }
}
