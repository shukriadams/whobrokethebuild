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

        void IPostProcessorPlugin.VerifyJobConfig(Job job)
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(IPostProcessorPlugin.VerifyJobConfig),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
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
