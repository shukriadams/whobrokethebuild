using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web.Core
{
    /// <summary>
    /// Runs import build and import log on build systems.
    /// </summary>
    public class LogImportDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;
        
        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public LogImportDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;
            _di = new SimpleDI();
            _pluginProvider = _di.Resolve<PluginProvider>();
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _processRunner.Start(new DaemonWorkThreaded(this.WorkThreaded), tickInterval, this, DaemonTaskTypes.LogImport);
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
            BuildServer buildServer = dataRead.GetBuildServerByKey(job.BuildServer);
            IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
            ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

            if (!reach.Reachable)
            {
                _log.LogError($"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Blocked, Description = $"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}" };
            }

            BuildLogRetrieveResult result = buildServerPlugin.ImportLog(build);
            task.Result = result.Result;

            if (!result.Success)
                return new DaemonTaskWorkResult { ResultType=DaemonTaskWorkResultType.Failed, Description = result.Result };

            build.LogFetched = true;
            dataWrite.SaveBuild(build);

            // create tasks for next stage
            foreach (string logparser in job.LogParsers)
                dataWrite.SaveDaemonTask(new DaemonTask
                {
                    BuildId = build.Id,
                    Src = this.GetType().Name,
                    Args = logparser,
                    Stage = (int)DaemonTaskTypes.LogParse
                });

            // build revision requires source control
            if (!string.IsNullOrEmpty(job.RevisionAtBuildRegex) && !string.IsNullOrEmpty(job.SourceServerId))
                dataWrite.SaveDaemonTask(new DaemonTask
                {
                    BuildId = build.Id,
                    Src = this.GetType().Name,
                    Stage = (int)DaemonTaskTypes.RevisionFromLog
                });

            ConsoleHelper.WriteLine(this, $"Log imported for build {build.Key} (id:{build.Id})");

            return new DaemonTaskWorkResult { };
        }

        #endregion
    }
}
