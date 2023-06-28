using System;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.Test2
{
    public class Test2 : Plugin, IPostProcessor
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
            throw new Exception("it failed");
        }
    }
}
