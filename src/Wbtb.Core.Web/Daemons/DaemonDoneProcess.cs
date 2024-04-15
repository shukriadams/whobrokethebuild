using System;

namespace Wbtb.Core.Web
{
    public class DaemonDoneProcess
    {
        public DateTime DoneUTc { get; set; }
        public string TaskId { get; set; }
        public string BuildId { get; set; }
        public string Daemon { get; set; }
    }
}
