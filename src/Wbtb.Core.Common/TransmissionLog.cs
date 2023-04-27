using System;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Log of transmission to recipient to inform of build event. This is used as both a log and for flood prevention.
    /// </summary>
    public class TransmissionLog
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The build which the event belongs to.
        /// </summary>
        public string BuildId { get; set; }

        /// <summary>
        /// Unique string describing contact carrier, egs, email, slack, etc
        /// </summary>
        public string CarrierContext { get; set; }
    
        /// <summary>
        /// Some unique string used to identifier receiver (email address, network id, slack channel id)
        /// </summary>
        public string ReceiverContext { get; set; }

        /// <summary>
        /// Some unique string describing the event being alerted to
        /// </summary>
        public string EventContext { get; set; }

        /// <summary>
        /// Date transmission was sent.
        /// </summary>
        public DateTime CreatedUtc { get; set; }
    }
}
