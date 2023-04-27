using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildHostModel : LayoutModel
    {
        public string Hostname { get; set; }

        public string BaseUrl { get; set; }

        public PageableData<ViewBuild> Builds { get; set; }
    }
}
