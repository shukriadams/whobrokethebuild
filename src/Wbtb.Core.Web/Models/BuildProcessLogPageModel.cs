using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildProcessPageModel : LayoutModel
    {
        #region PROPERTIES

        public IEnumerable<DaemonTask> DaemonTasks { get; set; }

        public Build Build { get; set; }

        #endregion

        #region CTORS

        public BuildProcessPageModel()
        {
            this.DaemonTasks = new List<DaemonTask>();
        }

        #endregion
    }
}
