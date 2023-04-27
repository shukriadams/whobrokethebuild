using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildProcessorLogPageModel : LayoutModel
    {
        public string Log { get; set; }

        public Build Build { get; set; }

        public BuildProcessor BuildProcessor { get; set; }
    }
}
