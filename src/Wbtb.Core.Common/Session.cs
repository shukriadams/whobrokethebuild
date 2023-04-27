using System;

namespace Wbtb.Core.Common
{
    public class Session
    {
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreatedUtc { get; set; }
    }
}
