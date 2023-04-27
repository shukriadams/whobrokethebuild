namespace Wbtb.Core.Common.Plugins
{
    public class LogParserProxy : PluginProxy, ILogParser
    {
        public string Parse(string raw)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            return pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "Parse",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "raw", Value = raw }
                }
            });
        }
    }
}
