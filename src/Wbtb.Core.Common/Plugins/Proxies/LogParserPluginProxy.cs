namespace Wbtb.Core.Common
{
    public class LogParserPluginProxy : PluginProxy, ILogParserPlugin
    {
        private readonly IPluginSender _pluginSender;

        void IPlugin.Diagnose()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(IPlugin.Diagnose)
            });
        }

        public LogParserPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        void ILogParserPlugin.Parse(string raw)
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(ILogParserPlugin.Parse),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "raw", Value = raw }
                }
            });
        }

        LogParsePickupResult ILogParserPlugin.Pickup(string raw)
        {
            return _pluginSender.InvokeMethod<LogParsePickupResult>(this, new PluginArgs
            {
                FunctionName = nameof(ILogParserPlugin.Pickup),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "raw", Value = raw }
                }
            });
        }
    }
}
