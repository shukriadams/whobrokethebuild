namespace Wbtb.Core.Common
{
    public class BuildLevelProcessorProxy : PluginProxy, IBuildLevelProcessor
    {
        private readonly IPluginSender _pluginSender;

        public BuildLevelProcessorProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        public void Diagnose()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "Diagnose"
            });
        }

        public void Process(Build build)
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "Process",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }
    }
}
