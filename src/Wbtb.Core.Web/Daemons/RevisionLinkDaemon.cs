﻿using Microsoft.Extensions.Logging;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Imports full revision objects from source control system, creates them in DB, and links them to buildinvolvements
    /// where applicable.
    /// </summary>
    public class RevisionLinkDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;

        private readonly Configuration _config;
        
        private readonly PluginProvider _pluginProvider;
        
        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public RevisionLinkDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            _di = new SimpleDI();
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>();
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _processRunner.Start(new DaemonWorkThreaded(this.WorkThreaded), tickInterval, this, DaemonTaskTypes.RevisionLink);
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
            BuildInvolvement buildInvolvement = dataRead.GetBuildInvolvementById(task.BuildInvolvementId);
            SourceServer sourceServer = dataRead.GetSourceServerByKey(job.SourceServer);
            ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;

            if (!sourceServerPlugin.AttemptReach(sourceServer).Reachable)
            {
                _log.LogError($"unable to reach source server \"{sourceServer.Name}\", waiting for later.");
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Blocked, Description = $"source server {sourceServer.Name} unreachable" };
            }

            RevisionLookup revisionLookup = sourceServerPlugin.GetRevision(sourceServer, buildInvolvement.RevisionCode);
            if (!revisionLookup.Success)
                return new DaemonTaskWorkResult {  ResultType = DaemonTaskWorkResultType.Failed, Description = $"Failed to resolve revision {buildInvolvement.RevisionCode} from source control server." };

            revisionLookup.Revision.SourceServerId = sourceServer.Id;
            dataWrite.SaveRevision(revisionLookup.Revision);

            buildInvolvement.RevisionId = revisionLookup.Revision.Id;
            dataWrite.SaveBuildInvolement(buildInvolvement);

            ConsoleHelper.WriteLine(this, $"Linked revision {revisionLookup.Revision.Code} to build {build.Key} (id:{build.Id})");

            return new DaemonTaskWorkResult();
        }

        #endregion
    }
}
