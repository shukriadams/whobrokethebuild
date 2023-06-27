﻿using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class ProcessPageModel : LayoutModel
    {
        #region PROPERTIES

        public PageableData<ViewDaemonTask> DaemonTasks { get; set; }

         public IEnumerable<DaemonActiveProcessItem> ActiveProcesses { get; set; }
        public string BaseUrl { get; set; }

        public string QueryStrings { get; set; }

        #endregion

        #region CTORS

        public ProcessPageModel()
        {
            this.DaemonTasks = new PageableData<ViewDaemonTask>(new ViewDaemonTask[] { }, 0, 0, 0);
            this.ActiveProcesses = new DaemonActiveProcessItem [] { };
            this.QueryStrings = string.Empty;
        }

        #endregion
    }
}