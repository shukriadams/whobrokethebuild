using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildLogParseResultsPageModel : LayoutModel
    {
        public ViewBuild Build { get; set; }

        public IEnumerable<BuildLogParseResult> LogParseResults { get; set; }

        public ViewJobBanner Banner { get; set; }

        public string Raw { get; set; }
    }
}
