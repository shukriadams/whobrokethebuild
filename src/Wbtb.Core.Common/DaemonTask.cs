using System;

namespace Wbtb.Core.Common
{
    public class DaemonTask : ISignature
    {
        #region PROPERTIES

        /// <summary>
        /// Id of record in database
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Build task belongs to
        /// </summary>
        public string BuildId { get; set; }

        /// <summary>
        /// Optional.
        /// </summary>
        public string BuildInvolvementId { get; set; }

        /// <summary>
        /// Additional config information carried by task. Normally written by task create process, and then consumer by task execution process.
        /// </summary>
        public string Args { get; set; }

        /// <summary>
        /// Daemon that wrote the task
        /// </summary>
        public string Src { get; set; }

        /// <summary>
        /// Time task was queued. 
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Null if task hasn't processed yet.
        /// </summary>
        public DateTime? ProcessedUtc { get; set; }

        /// <summary>
        /// Null of task hasn't processed yet.
        /// </summary>
        public bool? HasPassed { get; set; }

        /// <summary>
        /// String describing result of task. For debugging / logging.
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// Execution order of task.
        /// </summary>
        public int Stage { get; set; }

        /// <summary>
        /// Id of the daemontask that caused this task to fail. Null if this task failed on its own.
        /// </summary>
        public string FailDaemonTaskId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Signature { get; set; }

        #endregion

        #region CTORS

        public DaemonTask() 
        {
            this.Signature = Guid.NewGuid().ToString();
            this.CreatedUtc = DateTime.UtcNow;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Convenient newline append to result
        /// </summary>
        /// <param name="addition"></param>
        public void AppendResult(object addition) 
        {
            this.Result += $"{addition}\n";
        }

        #endregion

    }
}
