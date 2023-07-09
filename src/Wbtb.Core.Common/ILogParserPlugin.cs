namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(LogParserPluginProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface ILogParserPlugin : IPlugin
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

        /// <summary>
        /// Parses a log, caches the result locally. Result can be retrieved when Parse() is called. This method is meant for
        /// multithreaded "burst" parse runs.
        /// </summary>
        /// <param name="raw"></param>
        string ParseAndCache(string raw);
    }
}
