using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class UsersPageModel : LayoutModel
    {
        public IEnumerable<User> Users {get;set; }

        public UsersPageModel()
        { 
            this.Users = new List<User>();
        }
    }
}
