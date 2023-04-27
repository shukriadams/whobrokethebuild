using System;

namespace Wbtb.Core.Common
{
    public class BuildProcessor : ISignature
    {
        /// <summary>
        /// Unique, immutable id in local infrastructure. Defined in config file for local setup. Will be used as parentID for child records. 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Required
        /// </summary>
        public string BuildId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BuildProcessorStatus Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ProcessorKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Signature { get; set; }

        public BuildProcessor()
        {
            this.Signature = Guid.NewGuid().ToString();
        }
    }
}
