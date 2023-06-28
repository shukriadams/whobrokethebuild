using System;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.Test
{
    public class Test : Plugin, IPostProcessor
    {
        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        PostProcessResult IPostProcessor.Process()
        { 
            return new PostProcessResult { 
                Passed = true,
                Result = "it worked"
            };
            
        }
    }
}
