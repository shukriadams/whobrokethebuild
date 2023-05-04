namespace Wbtb.Core.Common.Plugins
{
    public class LogParserProxy : PluginProxy, ILogParser
    {
        private readonly IPluginSender _pluginSender;

        public LogParserProxy(IPluginSender pluginSender) : base(pluginSender)
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
