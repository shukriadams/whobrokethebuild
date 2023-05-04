using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class PluginPartitionedConfig 
    {
        /// <summary>
        /// Plugin Id
        /// </summary>
        public string Plugin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Config { get; set; }
    }
}
