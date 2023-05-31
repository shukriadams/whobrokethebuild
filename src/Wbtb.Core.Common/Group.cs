using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class Group: IIdentifiable
    {
        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string KeyPrev { get; set; }
         
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Raw user initials from config file
        /// </summary>
        public IList<string> Users { get; set; }

        /// <summary>
        /// Plugin-specific configuration for this user. 
        /// </summary>
        public IList<MessageConfiguration> Message { get; set; }

        #endregion

        #region CTORS

        public Group()
        {
            this.Users = new List<string>();
            this.Message = new List<MessageConfiguration>();
            this.Enable = true;
        }

        #endregion
    }
}
