namespace Wbtb.Core.Common.Plugins
{
    public abstract class PluginProxy : IPluginProxy
    {
        private readonly IPluginSender _pluginSender;

        public string PluginKey { get; set; }

        public PluginConfig ContextPluginConfig { get; set; }

        public PluginProxy(IPluginSender pluginSender) 
        {
            _pluginSender = pluginSender;
        }

        public void InjectConfig(PluginConfig config)
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "InjectConfig",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "config", Value = config }
                }
            });
        }

        public PluginInitResult InitializePlugin()
        {
            return _pluginSender.InvokeMethod<PluginInitResult>(this, new PluginArgs
            {
                FunctionName = "InitializePlugin"
            });
        }

        public ReachAttemptResult AttemptReach()
        {
            return _pluginSender.InvokeMethod<ReachAttemptResult>(this, new PluginArgs
            {
                FunctionName = "VerifyCredentials"
            });
        }

    }
}
