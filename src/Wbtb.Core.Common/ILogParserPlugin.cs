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
        /// <param name="build">Build object log is being processed for.</param>
        /// <param name="raw">Raw string of build log.</param>
        /// <returns>A string of parsed results from raw log. String can be simple or be XML-like markup.</returns>
        string Parse(Build build, string raw);
    }
}
