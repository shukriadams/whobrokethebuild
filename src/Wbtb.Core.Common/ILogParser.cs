namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(LogParserProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface ILogParser : IPlugin
    {
        string Parse(string raw);
    }
}
