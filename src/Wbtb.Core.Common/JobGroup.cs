using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// A set of jobs for which a combined status can be queried. The group's status will return if 
    /// </summary>
    public class JobGroup
    {
        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Unique ids of jobs that are included in group.
        /// </summary>
        public IEnumerable<string> Jobs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public JobGroupBehaviour Behaviour { get; set; }

        #endregion

        #region CTORS

        public JobGroup()
        {
            this.Jobs = new List<string>();
        }

        #endregion
    }
}
