using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(SourceServerPluginProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface ISourceServerPlugin : IPlugin
    {
        ReachAttemptResult AttemptReach(SourceServer contextServer);

        /// <summary>
        /// Verifies sourceserver static config (not plugin)
        /// </summary>
        /// <param name="contextServer"></param>
        void VerifySourceServerConfig(SourceServer contextServer);

        void VerifyJobConfig(Job job);

        IEnumerable<Revision> GetRevisionsBetween(Job job, string revisionStart, string revisionEnd);

        Revision GetRevision(SourceServer contextServer, string revisionCode);
    }
}
