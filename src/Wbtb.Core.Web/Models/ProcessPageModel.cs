using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class ProcessPageModel : LayoutModel
    {
        #region PROPERTIES

        public PageableData<ViewDaemonTask> DaemonTasks { get; set; }

        public IEnumerable<DaemonActiveProcessItem> ActiveProcesses { get; set; }

        public IList<DaemonBlockedProcessItem> BlockedProcesses { get; set; }

        public string BaseUrl { get; set; }

        public string QueryStrings { get; set; }

        public string FilterBy { get; set; }

        public string OrderBy { get; set; }

        public string JobId { get; set; }

        public IEnumerable<Job> Jobs { get; set; }

        #endregion

        #region CTORS

        public ProcessPageModel()
        {
            this.DaemonTasks = new PageableData<ViewDaemonTask>(new ViewDaemonTask[] { }, 0, 0, 0);
            this.ActiveProcesses = new DaemonActiveProcessItem [] { };
            this.BlockedProcesses = new DaemonBlockedProcessItem[] { };
            this.QueryStrings = string.Empty;
            this.FilterBy = string.Empty;
            this.OrderBy = string.Empty;
            this.Jobs = new Job[] { };
        }

        #endregion
    }
}
