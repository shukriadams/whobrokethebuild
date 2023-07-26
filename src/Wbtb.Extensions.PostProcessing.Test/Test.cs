using System;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.Test
{
    public class Test : Plugin, IPostProcessorPlugin
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
            return new PostProcessResult { 
                Passed = true,
                Result = "it worked"
            };
            
        }
    }
}
