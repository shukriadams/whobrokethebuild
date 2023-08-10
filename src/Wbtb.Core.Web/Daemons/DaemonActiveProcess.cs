using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class DaemonActiveProcess
    {
        public DateTime CreatedUtc { get; set; }

        public Type Daemon { get; set; }

        public Build Build { get; set; }

        public DaemonTask Task { get; set; }

        public string Description { get; set; }
    }
}
