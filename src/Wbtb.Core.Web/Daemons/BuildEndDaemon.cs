using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildEndDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        private readonly Configuration _configuration;

        #endregion

        #region CTORS

        public BuildEndDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            _di = new SimpleDI();
            _pluginProvider = _di.Resolve<PluginProvider>();
            _configuration = _di.Resolve<Configuration>();
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _processRunner.Start(new DaemonWorkThreaded(this.WorkThreaded), tickInterval, this, DaemonTaskTypes.BuildEnd);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _processRunner.Dispose();
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

            return new DaemonTaskWorkResult();
        }

        #endregion
    }
}
