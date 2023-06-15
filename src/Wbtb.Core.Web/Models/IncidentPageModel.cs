using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class IncidentPageModel : LayoutModel
    {
        public Job Job { get; set; }
        public bool IsActive { get; set; }

        public Build IncidentBuild { get; set; }

        public IEnumerable<Build> InvolvedBuilds { get; set; }
    }
}
