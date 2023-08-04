using System.Collections.Generic;
using System;

namespace Wbtb.Core.Web
{
    public class DaemonBlockedProcess
    {
        public DateTime CreatedUtc { get; set; }

        public string TaskId { get; set; }

        public string Reason { get; set; }

        public IEnumerable<string> BlockingProcesses { get; set; }
    }
}
