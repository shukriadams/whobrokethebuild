namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(LogParserProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface ILogParser : IPlugin
    {
        /// <summary>
        /// Returns subsection of log that matches parser's lookup purpose. Subsection can be simple text, or can
        /// be formatted in WBTB's log format
        /// 
        /// <x-logParseLine>
        ///     <x-logParseItem>...</x-logParseItem>
        ///     <x-logParseItem>...</x-logParseItem>
        /// </x-logParseLine>
        /// 
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        string Parse(string raw);
    }
}
