using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class SessionViewModel
    {
        /// <summary>
        /// Rolls the user has assigned to them. Pages / objects can impliment roll requirement. Rolls are like "groups". Empty if not logged in.
        /// </summary>
        public IEnumerable<string> Rolls { get;set; }

        /// <summary>
        /// True if the user has a valid session, ie, is logged in.
        /// </summary>
        public bool IsLoggedIn { get; set; }

        /// <summary>
        /// User's display name. Null if not logged in. 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If logged in, the user's id. Null if not logged in.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Objects / pages the user owns. Empty if not logged in.
        /// </summary>
        public IEnumerable<string> Ownerships { get; set; }
    }
}
