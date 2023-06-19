using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class User : IIdentifiable
    {
        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Config-defined unique id. required.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string KeyPrev { get; set; }

        /// <summary>
        /// human-friend name of user. Set to key if not set.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Human-friendly initials of user. Set to first three chars of name if not set.
        /// </summary>
        public string Initials { get; set; }

		/// <summary>
		/// Optional image url for user
		/// </summary>
		public string Image { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Plugin to authenticate (login) user 
        /// </summary>
        public string AuthPlugin { get; set; } 

        /// <summary>
        /// 
        /// </summary>
        public IList<UserSourceIdentity> SourceServerIdentities { get; set; }

        /// <summary>
        /// Methods to communicate with user
        /// </summary>
        public IList<MessageConfiguration> Message { get; set; }

        #endregion

        #region CTORS

        public User ()
        { 
            this.Enable = true;
            this.SourceServerIdentities = new List<UserSourceIdentity>();
            this.Message = new List<MessageConfiguration>();
        }

        #endregion
    }
}
