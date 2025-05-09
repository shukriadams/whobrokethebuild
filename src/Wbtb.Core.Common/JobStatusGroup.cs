using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// A set of jobs for which a combined status can be queried. The group's status will return if 
    /// </summary>
    public class JobStatusGroup
    {
        /// <summary>
        /// 
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Unique ids of jobs that are included in group.
        /// </summary>
        public IEnumerable<string> JobKeys { get; set; }

        /// <summary>
        /// Display name of group
        /// </summary>
        public string Name { get; set; }

        public JobStatusGroupConditions Behaviour { get; set; }
    }
}
