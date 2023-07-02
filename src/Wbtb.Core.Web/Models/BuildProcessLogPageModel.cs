using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildProcessPageModel : LayoutModel
    {
        #region PROPERTIES

        public IEnumerable<DaemonTask> DaemonTasks { get; set; }

        public Build Build { get; set; }

        public bool IsComplete { get; set; }

        public bool HasErrors { get; set; }

        public TimeSpan? QueueTime { get; set; }

        #endregion

        #region CTORS

        public BuildProcessPageModel()
        {
            this.DaemonTasks = new List<DaemonTask>();
        }

        #endregion
    }
}
