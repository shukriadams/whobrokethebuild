using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class Job : IIdentifiable
    {
        #region PROPERTIES

        /// <summary>
        /// Internal database id. Never refer to this externally, it can change if database is rebuilt. Use key for external references.
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// Semi-immutable unique id, in that it's defined in local config and should not change. Try to use a random string instead
        /// of a human-friendly name, as human-friendly names can change, but key must exist for permalinks and references to live on.
        /// If for some reason this has to be changed in config, use the CLI to migrate existing recordings to the new key.
        /// 
        /// Use Name property for a human-friendly label.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Cosmetic human-friendly name for job. Can be changed without breaking anything, there is no logic bound to this.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Used only when for Key migrating.
        /// </summary>
        public string KeyPrev { get; set; }

        /// <summary>
        /// Like Name, but longer. Human-friendly 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Config-defined Key of CI server this job is attached to
        /// </summary>
        public string BuildServer {  get;set;}

        /// <summary>
        /// Database id of build server. Can change on database rebuild.
        /// </summary>
        public string BuildServerId { get; set; }

        /// <summary>
        /// If true, revisions will be linked to build event (either via build server or log regex). If false, several processors in the process chain 
        /// will be bypassed. 
        /// </summary>
        public bool LinkRevisions { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public IList<MessageHandler> Message { get; set; }

        /// <summary>
        /// Config-defined key of version control server this job builds code for
        /// </summary>
        public string SourceServer { get; set; }

        /// <summary>
        /// Database id of SourceServer record.
        /// </summary>
        public string SourceServerId { get; set; }

        /// <summary>
        /// Number of builds to import for this job. Overrides count set at build server. If not set falls back to default. 
        /// </summary>
        public int ImportCount { get; set; }

        /// <summary>
        /// Optional. Keys of log parser plugins to use to process build logs for this job
        /// </summary>
        public IEnumerable<string> LogParsers { get; set; }

        /// <summary>
        /// optional. Keys of postProcessor plugins.
        /// </summary>
        public IEnumerable<string> PostProcessors { get; set; }

        /// <summary>
        /// Plugins to be invoked when a build record is created.
        /// </summary>
        public IEnumerable<string> OnBuildStart { get; set; }

        /// <summary>
        /// Plugins to be invoked when a build has completed, and after its log has been imported.
        /// </summary>
        public IEnumerable<string> OnBuildEnd { get; set; }

        /// <summary>
        /// Plugins to be invoked when a build's log is imported. This happens after build has completed.
        /// </summary>
        public IEnumerable<string> OnLogAvailable { get; set; }

        public IEnumerable<string> OnFixed { get; set; }

        public IEnumerable<string> OnBroken { get; set; }

        /// <summary>
        /// If false, job will no be processed. 
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// zero by default. If greater than zero, the number of builds back in time to import. Useful when pointing to an existing job with many builds. 
        /// </summary>
        public int HistoryLimit { get; set; }

        /// <summary>
        /// Path to image file - can be local or remote (absolute http path required)
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Some jobs do not expose revisions in build via build server API. In those cases, it might be disarable to emit the revision in a given build in the build
        /// log. This regex string, if set, can be used to detect the revision once build logs have been fetched. 
        /// </summary>
        public string RevisionAtBuildRegex { get; set; }

        /// <summary>
        /// If RevisionAtBuildRegex is being used, and this is set to true, all revisions since last revision in previous build will be assumed to be part of current build.
        /// </summary>
        public bool RevisionScrapeSpanBuilds { get; set; }

        /// <summary>
        /// Addition key-value config specific to plugin. These are defined in config.yml
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Config { get; set; }

        /// <summary>
        /// Returns if job supports showing revisions from build log
        /// </summary>
        public bool CanHaveRevisionInBuildLog 
        {
            get 
            {
                return !string.IsNullOrEmpty(this.RevisionAtBuildRegex);
            }
        }

        #endregion

        #region CTORS

        public Job()
        {
            this.Message = new List<MessageHandler>();
            this.LogParsers = new List<string>();
            this.PostProcessors = new List<string>();
            this.OnBuildStart = new List<string>();
            this.OnBuildEnd = new List<string>();
            this.OnLogAvailable= new List<string>();
            this.OnFixed = new List<string>();
            this.OnBroken = new List<string>();
            this.Enable = true;
            this.ImportCount = 20;
        }

        #endregion
    }
}
