using Microsoft.Extensions.Logging;
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

            Revision revision = sourceServerPlugin.GetRevision(sourceServer, buildInvolvement.RevisionCode);
            if (revision == null)
                return new DaemonTaskWorkResult {  ResultType = DaemonTaskWorkResultType.Failed, Description = $"Failed to resolve revision {buildInvolvement.RevisionCode} from source control server." };

            revision.SourceServerId = sourceServer.Id;
            dataWrite.SaveRevision(revision);

            buildInvolvement.RevisionId = revision.Id;
            dataWrite.SaveBuildInvolement(buildInvolvement);

            return new DaemonTaskWorkResult();
        }

        #endregion
    }
}
