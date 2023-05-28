using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class UserPageModel : LayoutModel
    {
        /// <summary>
        /// User object if found. Can be be null.
        /// </summary>
        public new User User { get;set; }

        /// <summary>
        /// String username. Will be set if User object cannot be found. Matches the name of user in source control commits, and _may_ therefore be
        /// source server specific.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ViewBuildInvolvement> RecentBreaking { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ViewBuildInvolvement> RecentPassing { get; set; }

        #region CTORS

        public UserPageModel()
        { 
            this.RecentBreaking = new List<ViewBuildInvolvement>();

            this.RecentPassing = new List<ViewBuildInvolvement>();
        }

        #endregion
    }
}
