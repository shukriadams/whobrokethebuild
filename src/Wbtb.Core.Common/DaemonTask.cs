using System;

namespace Wbtb.Core.Common
{
    public class DaemonTask : ISignature
    {
        /// <summary>
        /// Id of record in database
        /// </summary>
        public string Id { get; set; }

        public string BuildId { get; set; }

        /// <summary>
        /// Optional.
        /// </summary>
        public string BuildInvolvementId { get; set; }

        /// <summary>
        /// Identifier used by daemons to pick up tasks
        /// </summary>
        public string TaskKey { get; set; }

        /// <summary>
        /// Daemon that wrote the task
        /// </summary>
        public string Src { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime? ProcessedUtc { get; set; }

        public bool? HasPassed { get; set; }

        public string Result { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Signature { get; set; }

        public DaemonTask() 
        {
            this.Signature = Guid.NewGuid().ToString();
            this.CreatedUtc = DateTime.UtcNow;
        }
    }
}
