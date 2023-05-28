using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class JobIncidentsModel : LayoutModel
    {
        public string BaseUrl { get; set; }

        public PageableData<ViewBuild> Builds { get; set; }

    }
}
