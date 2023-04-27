using System;

namespace Wbtb.Core.Common
{
    public class ConfigurationState
    {
        public virtual string Id { get; set; }

        public DateTime CreatedUtc { get; set; }

        public string Hash { get; set; }

        public string Content { get; set; }
    }
}
