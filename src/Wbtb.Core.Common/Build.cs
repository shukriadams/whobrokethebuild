using System;

namespace Wbtb.Core.Common
{
    public class Build : ISignature
    {
        /// <summary>
        /// Id of record in database
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Parent job this build is associated with
        /// </summary>
        public string JobId { get; set; }

        /// <summary>
        /// Unique, immutable id from CI server this build originates from. Will be used to trace this record back to that build.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Hash/revision/id/etc of code change that triggered build. 
        /// </summary>
        public string TriggeringCodeChange { get; set; }

        /// <summary>
        /// Type of event that triggered build (manual, code change, timer etc). This can't be constrained so can't have any logic associated with it, and is for 
        /// information only.
        /// </summary>
        public string TriggeringType { get; set; }

        /// <summary>
        /// Time build started
        /// </summary>
        public DateTime StartedUtc { get; set; }

        /// <summary>
        /// Set when build is confirmed exited.
        /// </summary>
        public DateTime? EndedUtc { get; set; }

        /// <summary>
        /// Name of machine build ran on, if known.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BuildStatus Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BuildDelta Delta { get; set; }

        /// <summary>
        /// If build has not passed, it must be assigned an incident build, which is the the first build _after_ the last preceeding build that passed. This
        /// can be this build if the preceeding build passed. This build id is assigned by a daemon, and will eventually always be set if the build status is
        /// not pasing. Aborted builds count as incidents too.
        /// </summary>
        public string IncidentBuildId { get; set; }

        /// <summary>
        /// Path log is stored to on local file system. Null until log is fetched.
        /// </summary>
        public string LogPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Signature { get; set; }

        public Build()
        {
            this.Signature = Guid.NewGuid().ToString();
        }
    }
}
