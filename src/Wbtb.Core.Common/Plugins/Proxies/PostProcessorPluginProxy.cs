namespace Wbtb.Core.Common
{
    public class PostProcessorPluginProxy : PluginProxy, IPostProcessorPlugin
    {
        private readonly IPluginSender _pluginSender;

        public PostProcessorPluginProxy(IPluginSender pluginSender) : base(pluginSender)
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

        PostProcessResult IPostProcessorPlugin.Process(Build build)
        {
            return _pluginSender.InvokeMethod<PostProcessResult>(this, new PluginArgs
            {
                FunctionName = nameof(IPostProcessorPlugin.Process),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }
    }
}
