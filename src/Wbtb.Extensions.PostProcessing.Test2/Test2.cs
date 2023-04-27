using System;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.PostProcessing.Test2
{
    public class Test2 : Plugin, IPostProcessor
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
            throw new Exception("it failed");
        }
    }
}
