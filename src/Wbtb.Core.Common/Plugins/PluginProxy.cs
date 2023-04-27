namespace Wbtb.Core.Common.Plugins
{
    public abstract class PluginProxy : IPluginProxy
    {
        public string PluginKey { get; set; }

        public PluginConfig ContextPluginConfig { get; set; }

        public void InjectConfig(PluginConfig config)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "InjectConfig",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "config", Value = config }
                }
            });
        }

        public PluginInitResult InitializePlugin()
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            return pluginSender.InvokeMethod<PluginInitResult>(this, new PluginArgs
            {
                FunctionName = "InitializePlugin"
            });
        }

        public ReachAttemptResult AttemptReach()
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            return pluginSender.InvokeMethod<ReachAttemptResult>(this, new PluginArgs
            {
                FunctionName = "VerifyCredentials"
            });
        }

    }
}
