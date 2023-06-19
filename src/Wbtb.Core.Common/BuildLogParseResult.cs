namespace Wbtb.Core.Common
{
    public class BuildLogParseResult
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// The build a parse result is associated with. Required.
        /// </summary>
        public virtual string BuildId { get; set; }

        /// <summary>
        /// Optionally, a parse result also can be connected with a revision and therefore a user, which is done via 
        /// a build involvement. 
        /// </summary>
        public virtual string BuildInvolvementId { get; set; }

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
