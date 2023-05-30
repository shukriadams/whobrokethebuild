using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class JobPageModel : LayoutModel
    {
        public string Title { get; set; }

        public string BaseUrl { get; set; }

        public ViewJob Job { get; set; }

        public JobStats Stats { get; set; }

        public PageableData<ViewBuild> Builds { get; set; }

    }
}
