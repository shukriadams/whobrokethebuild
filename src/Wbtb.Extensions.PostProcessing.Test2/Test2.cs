using System;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.Test2
{
    public class Test2 : Plugin, IPostProcessorPlugin
    {
        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        void IPostProcessorPlugin.VerifyJobConfig(Job job)
        {

        }

        PostProcessResult IPostProcessorPlugin.Process(Build build)
        {
            throw new Exception("it failed");
        }
    }
}
