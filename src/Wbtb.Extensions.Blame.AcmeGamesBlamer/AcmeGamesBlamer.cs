using Wbtb.Core.Common;

namespace Wbtb.Extensions.Blame.AcmeGamesBlamer
{
    internal class AcmeGamesBlamer : Plugin, IBlamePlugin
    {
        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        void IBlamePlugin.BlameBuildFailure(Build failingBuild)
        {
            // do stuff here
        }
    }
}
