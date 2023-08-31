using System.Collections.Generic;
using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class DaemonBlockedProcess
    {
        public DateTime CreatedUtc { get; set; }

        public DaemonTask Task { get; set; }

        public Type Daemon { get; set; }

        public Build Build { get; set; }

        public string Reason { get; set; }

        public IEnumerable<DaemonTask> BlockingProcesses { get; set; }

        public int ErrorCount { get; set; }

        public DaemonBlockedProcess() 
        {
            ErrorCount = 1;
        }
    }
}
