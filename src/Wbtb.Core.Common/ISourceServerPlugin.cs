using System.Collections.Generic;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(SourceServerPluginProxy))]
    [PluginBehaviour(true)]
    public interface ISourceServerPlugin : IPlugin
    {
        ReachAttemptResult AttemptReach(SourceServer contextServer);

        /// <summary>
        /// Verifies sourceserver static config (not plugin)
        /// </summary>
        /// <param name="contextServer"></param>
        void VerifySourceServerConfig(SourceServer contextServer);

        IEnumerable<Revision> GetRevisionsBetween(SourceServer contextServer, string revisionStart, string revisionEnd);

        Revision GetRevision(SourceServer contextServer, string revisionCode);
    }
}
