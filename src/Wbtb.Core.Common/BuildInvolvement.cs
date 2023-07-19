using System;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Some kind of even that affects a build's outcome. Normally a revision created by a user, but can also be an interaction with a system, such as a build agent crashing.
    /// </summary>
    public class BuildInvolvement : ISignature
    {
        #region PROPERTIES

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
        /// Id of user if mapped locally. null if not. Calculated by resolving revision object using RevisionCode, then resolving user
        /// from username in that revision.
        /// </summary>
        public string MappedUserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsIgnoredFromBreakHistory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Score from 0 (innocent to 100, guaranteed blame. Everything in between is arbitrary.
        /// </summary>
        public int BlameScore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Signature { get; set; }

        #endregion

        #region CTORS

        public BuildInvolvement()
        {
            this.Signature = Guid.NewGuid().ToString();
        }

        #endregion
    }
}
