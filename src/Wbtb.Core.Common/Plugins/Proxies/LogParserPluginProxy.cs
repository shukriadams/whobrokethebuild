namespace Wbtb.Core.Common
{
    public class LogParserPluginProxy : PluginProxy, ILogParserPlugin
    {
        private readonly IPluginSender _pluginSender;

        public void Diagnose()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "Diagnose"
            });
        }

        public LogParserPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        public string Parse(string raw)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "Parse",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "raw", Value = raw }
                }
            });
        }
    }
}
