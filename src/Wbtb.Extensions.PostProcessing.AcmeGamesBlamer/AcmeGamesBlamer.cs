using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.AcmeGamesBlamer
{
    internal class AcmeGamesBlamer : Plugin, IPostProcessorPlugin
    {
        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        PostProcessResult IPostProcessorPlugin.Process(Build failingBuild)
        {
            return new PostProcessResult { };
        }
    }
}
