using System.Collections.Generic;

namespace Wbtb.Core.Common.Configuration
{
    public interface IConfigHolder
    {
        /// <summary>
        /// 
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IEnumerable<KeyValuePair<string, object>> Config { get; set; }
    }
}
