namespace Wbtb.Core.Common
{
    public abstract class PluginProxy : IPluginProxy
    {
        #region FIELDS

        private readonly IPluginSender _pluginSender;

        #endregion

        #region PROPERTIES

        public string PluginKey { get; set; }

        public PluginConfig ContextPluginConfig { get; set; }

        #endregion

        #region CTORS

        public PluginProxy(IPluginSender pluginSender) 
        {
            _pluginSender = pluginSender;
        }

        #endregion

        #region METHODS

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
                FunctionName = "AttemptReach"
            });
        }

        #endregion
    }
}
