using System;
using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class IncidentReport : ISignature
    {
        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// id of the first build in sequence of breaks
        /// </summary>
        public string IncidentId { get; set; }

        /// <summary>
        /// Id of the specific build this incident is attached to 
        /// </summary>
        public string MutationId { get; set; }

        /// <summary>
        /// Revision (source control unique codes) confirmed to be involved in incident
        /// </summary>
        public IEnumerable<string> ImplicatedRevisions { get; set; }

        /// <summary>
        /// status identifier for summary. Normally "break" or "mutate". This is a descriptive field to make it easier to see that breaks are changing.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// short summary of build break, intended for alerts etc, must instantly summarize issue
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Long descru
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Identifier of the processor that created summary.
        /// </summary>
        public string Processor { get; set; }

        /// <summary>
        /// Summary creation
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        #endregion

        #region CTORS

        public IncidentReport()
        {
            this.Signature = Guid.NewGuid().ToString();
            this.CreatedUtc = DateTime.UtcNow;
            this.ImplicatedRevisions = new List<string>();
        }

        #endregion
    }
}
