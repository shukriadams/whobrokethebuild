using System;
using System.IO;

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
        public string Key { get; set; }

        /// <summary>
        /// A local identifier for a build that is somewhat immutable across database purges. Used for external permalinks, URLs and persistent cache
        /// keys. Internally, this value consists of the parent job key + build identifer, one-way hashed.
        /// </summary>
        public string UniquePublicKey { get; set; }

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
        /// If build has not passed, it must be assigned an incident build, which is the the first build _after_ the last preceeding build that passed. This
        /// can be this build if the preceeding build passed. This build id is assigned by a daemon, and will eventually always be set if the build status is
        /// not pasing. Aborted builds count as incidents too.
        /// </summary>
        public string IncidentBuildId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool LogFetched { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Signature { get; set; }

        public Build()
        {
            this.Signature = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Builds that count towards definitive history must either pass or fail, we don't care about builds that 
        /// have other statuses.
        /// </summary>
        /// <returns></returns>
        public bool IsDefinitive() 
        {
            if (this.Status == BuildStatus.Passed || this.Status == BuildStatus.Failed)
                return true;

            return false;
        }

        public void SetUniquePublicIdentifier(Job job) 
        {
            if (string.IsNullOrEmpty(this.Key))
                throw new Exception($"Cannot set public identifier on build id {this.Id}, build has no key value");

            if (string.IsNullOrEmpty(job.Key))
                throw new Exception($"Cannot set public identifier on build id {this.Id}, job id {job.Id} has no key value");

            this.UniquePublicKey = Sha256.FromString($"{this.Key}_{job.Key}");
        }

        public static string GetLogPath(Configuration config, Job job, Build build) 
        {
            return Path.Combine(config.BuildLogsDirectory, job.Key, build.Key, $"log.txt");
        }
    }
}
