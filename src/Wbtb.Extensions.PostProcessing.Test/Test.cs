using System;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.Test
{
    public class Test : Plugin, IPostProcessor
    {
        public PluginInitResult InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        public PostProcessResult Process()
        { 
            return new PostProcessResult { 
                Passed = true,
                Result = "it worked"
            };
            
        }
    }
}
