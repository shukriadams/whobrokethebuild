using System;

namespace Wbtb.Core.Common
{

    public class BuildInvolvement : ISignature
    {
        /// <summary>
        /// Database primary key index of record.
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// Database id of the build this involvement happens under.
        /// </summary>
        public string BuildId { get; set; }

        /// <summary>
        /// String representation of revision guid/hash/id etc. This is always known, and along with BuildId forms primary key
        /// </summary>
        public string RevisionCode { get; set; }

        /// <summary>
        /// If true, revision's inclusion in build is inferred based on timeline and revision on build machine at time of build.
        /// </summary>
        public bool InferredRevisionLink { get; set; }

        /// <summary>
        /// Id of revision if resolved. else null.
        /// </summary>
        public string RevisionId { get; set; }

        /// <summary>
        /// First link - uses revisionCode to look up revision from source control. Updates "User" field on this record.
        /// </summary>
        public LinkState RevisionLinkStatus { get;set; }

        /// <summary>
        /// Second link - uses "User" field on this object to try to link build involvement with a User record.
        /// </summary>
        public LinkState UserLinkStatus { get; set; }

        /// <summary>
        /// Id of user if mapped locally. null if not. Calculated by resolving revision object using RevisionCode, then resolving user
        /// from username in that revision.
        /// </summary>
        public string MappedUserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsIgnoredFromBreakHistory { get; set; }

        /// <summary>
        /// Member of Blame enum
        /// </summary>
        public Blame Blame { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Signature { get; set; }

        public BuildInvolvement()
        {
            this.Signature = Guid.NewGuid().ToString();
        }
    }
}
