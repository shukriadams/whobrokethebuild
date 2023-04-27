using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildFlagsModel : LayoutModel
    {
        public string BaseUrl { get; set;}

        public PageableData<ViewBuildFlag> BuildFlags{ get; set; }
    }
}
