namespace Wbtb.Core.Common
{
    public class BuildLogParseResult
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string BuildId { get; set; }

        /// <summary>
        /// Name of plugin which parsed the log
        /// </summary>
        public string LogParserPlugin { get; set; }

        /// <summary>
        /// Build logs can be parsed to retrieve meaningful content
        /// </summary>
        public string ParsedContent { get; set; }
    }
}
