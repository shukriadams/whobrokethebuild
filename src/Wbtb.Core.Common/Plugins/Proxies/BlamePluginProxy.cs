namespace Wbtb.Core.Common
{
    internal class BlamePluginProxy : PluginProxy, IBlamePlugin, IPluginProxy
    {
        #region FIELDS

        private readonly IPluginSender _pluginSender;

        #endregion

        #region CTORS

        public BlamePluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        #endregion

        void IPlugin.Diagnose()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(IPlugin.Diagnose)
            });
        }

        void IBlamePlugin.BlameBuildFailure(Build failingBuild)
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(IBlamePlugin.BlameBuildFailure),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "failingBuild", Value = failingBuild }
                }
            });
        }
    }
}
