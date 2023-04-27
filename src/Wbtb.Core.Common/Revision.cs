using System;
using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Revision represents a commit/revision in the original version control system. It contains primitives only.
    /// It doesn't contain a revision code. 
    /// Must cover all version control system
    /// </summary>
    public class Revision : ISignature
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual string Id { get; set;}

        /// <summary>
        /// Unique public identifier in the source. In Git this will be a commit hash, in perforce the changeid, etc
        /// </summary>
        public virtual string Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string SourceServerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual DateTime Created { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string User { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// file names in revision
        /// </summary>
        public IEnumerable<string> Files { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Signature { get; set; }

        public Revision()
        { 
            this.Files = new List<string>();
            this.Signature = Guid.NewGuid().ToString();
        }
    }
}
