using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    public class BuildLevelProcessorProxy : PluginProxy, IBuildLevelProcessor
    {
        public void Process(Build build)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "Process",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }
    }
}
