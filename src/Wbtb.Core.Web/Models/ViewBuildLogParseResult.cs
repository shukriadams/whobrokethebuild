using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class ViewBuildLogParseResult : BuildLogParseResult
    {
        public string InnerText { get; set; }

        public static ViewBuildLogParseResult Copy(BuildLogParseResult result)
        {
            BuildLogTextParser parser = new BuildLogTextParser();

            return new ViewBuildLogParseResult
            {
                BuildId = result.BuildId,
                BuildInvolvementId = result.BuildInvolvementId,
                Id = result.Id,
                InnerText = parser.ToInnerText(result),
                LogParserPlugin = result.LogParserPlugin,
                ParsedContent = result.ParsedContent,
                Signature = result.Signature
            };
        }

        public static IEnumerable<ViewBuildLogParseResult> Copy(IEnumerable<BuildLogParseResult> results)
        {
            IList<ViewBuildLogParseResult> items = new List<ViewBuildLogParseResult>();
            foreach(BuildLogParseResult result in results)
                items.Add(Copy(result));

            return items;
        }
    }
}
