﻿using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class ProcessPageModel : LayoutModel
    {
        #region PROPERTIES

        public PageableData<ViewDaemonTask> DaemonTasks { get; set; }

        public IEnumerable<DaemonActiveProcess> ActiveProcesses { get; set; }

        public IEnumerable<DaemonDoneProcess> DoneProcesses { get; set; }

        public IList<DaemonBlockedProcess> BlockedProcesses { get; set; }

        public IList<DaemonTask> BlockingDaemonTasks { get; set; }

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
            this.ActiveProcesses = new DaemonActiveProcess [] { };
            this.BlockedProcesses = new DaemonBlockedProcess[] { };
            this.BlockingDaemonTasks = new DaemonTask[] { };
            this.QueryStrings = string.Empty;
            this.FilterBy = string.Empty;
            this.OrderBy = string.Empty;
            this.Jobs = new Job[] { };
        }

        #endregion
    }
}
