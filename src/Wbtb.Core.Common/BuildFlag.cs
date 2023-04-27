using System;

namespace Wbtb.Core.Common
{
    public class BuildFlag
    {
        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Required
        /// </summary>
        public string BuildId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BuildFlags Flag { get; set; }

        /// <summary>
        /// optional
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Ignored { get; set; }

        #endregion

        #region CTORS
       
        public BuildFlag()
        { 
            this.Description = string.Empty;
            this.CreatedUtc = DateTime.UtcNow;
        }

        #endregion
    }
}
