namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(LogParserProxy))]
    [PluginBehaviour(false)]
    public interface ILogParser : IPlugin
    {
        string Parse(string raw);
    }
}
