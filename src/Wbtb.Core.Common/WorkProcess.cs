using System;

namespace Wbtb.Core.Common
{
    public class WorkProcess : IIdentifiable
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// Semi-immutable id, will be defined in config, and must also be immutable on build system
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string KeyPrev { get; set; }

        public string KeyNext { get; set; }

        public string Category { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime? KeptAliveUtc { get; set; }

        public string Content { get; set; }

        /// <summary>
        /// Milliseconds
        /// </summary>
        public int? Lifespan { get; set; }
    }
}
