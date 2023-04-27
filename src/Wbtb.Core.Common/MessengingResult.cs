namespace Wbtb.Core.Common
{
    public class MessengingResult
    {
        /// <summary>
        /// 
        /// </summary>
        public string MessageId { get; set;}

        /// <summary>
        /// 
        /// </summary>
        public bool Sent { get; set; }

        /// <summary>
        /// Description of result of send.
        /// </summary>
        public bool AttemptDescription { get; set; }
    }
}
