using System;
using System.Collections.Generic;
using System.Linq;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Main configuration. loaded from multiple sources.
    /// outermost is loaded from config.yml which is static and on-disk.
    /// PluginDeclarations are also static, but contain run-time data added on app start
    /// </summary>
    public class Configuration : ConfigurationBasic
    {
        #region PROPERTIES

        /// <summary>
        /// Hash of config text.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// If true, config error will be thrown if orphan records detected on startup checks.
        /// </summary>
        public bool FailOnOrphans { get; set; }

        /// <summary>
        /// Global default timer for daemons (seconds, will be converted to ms)
        /// </summary>
        public int DaemonInterval { get; set; }

        /// <summary>
        /// Maximum number of concurrent processes a daemon can run.
        /// </summary>
        public int MaxThreadsPerDaemon { get; set; }

        /// <summary>
        /// Time (seconds) after which daemon task will be marked as failed if processing or blocked
        /// </summary>
        public int DaemonTaskTimeout { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int DaemonMaxFailsPerTask { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int StandardPageSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PagesPerPageGroup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IAuthenticationPlugin DefaultAutProvider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int LiveConsoleSize { get; set; }

        /// <summary>
        /// Default : false. If true, all proxy calls to plugins will be written to stdout. This is for dev / debug only, and will flood your stdout.
        /// </summary>
        public bool LogOutgoingProxyCalls { get; set; }

        /// <summary>
        /// Forces sandbox mode on all plugins, prevent writes or changeing of state. Use this to test config changes on live services.
        /// </summary>
        public bool SandboxMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<BuildServer> BuildServers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<SourceServer> SourceServers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<User> Users { get; set; }

        /// <summary>
        /// Plugins defined in static config, and successfully loaded.
        /// </summary>
        public IList<PluginConfig> Plugins {get;set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Group> Groups { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<JobGroup> JobGroups { get; set; }
            
        /// <summary>
        /// Characters. Maximum length a log can be to processed by parsers. Safeguard against regex thread lockups. Logs that are too long are ignored by all parsers.
        /// </summary>
        public long MaxParsableLogSize { get; set; }

        /// <summary>
        /// Characters. Maximum length a line can be to processed by parsers. Safeguard against regex thread lockups. Lines that exceed this length are ignored by all parsers.
        /// </summary>
        public long MaxLineLength { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int UnprocessedBuildProcessorLimit { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsCurrentContextProxyPlugin { get;set;}

        /// <summary>
        /// For internal dev; set to true to to force server to connect to MessageQueue
        /// </summary>
        public bool ForceMessageQueue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; }

        public bool EnabledDaemons { get; set; }

        public bool EnabledSockets { get; set; }

        public int MaxThreads { get; set; }

        /// <summary>
        /// User comma-separated input
        /// </summary>
        public IEnumerable<string> FeatureToggles { get; set; }

        /// <summary>
        /// URL of server.
        /// </summary>
        public string Address { get; set; }

        #endregion

        #region CTORS

        public Configuration()
        {
            // defaults
            this.FailOnOrphans = true;
            this.StandardPageSize = 25;
            this.PagesPerPageGroup = 20;
            this.MaxParsableLogSize = 30000000;

            this.LiveConsoleSize = 500;
            this.DaemonInterval = 5;
            this.UnprocessedBuildProcessorLimit = 50;
            this.Port = 5000;
            this.EnabledDaemons = true;
            this.EnabledSockets = true;
            this.MaxThreads = 10;
            this.MaxThreadsPerDaemon = 2;
            this.DaemonMaxFailsPerTask = 500;
            this.DaemonTaskTimeout = 600; // 300 = 5 minutes

            this.Plugins = new List<PluginConfig>();
            this.BuildServers = new List<BuildServer>();
            this.SourceServers = new List<SourceServer>();
            this.Users = new List<User>();
            this.Groups = new List<Group>();
            this.JobGroups = new List<JobGroup>();
            this.FeatureToggles = EnvironmentVariableHelper.GetString("WBTB__FEATURE_TOGGLES", string.Empty).Split(",", StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Helper method to safely get value from KeyValue pair collections in a single line, with default value.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetConfigValue(IEnumerable<KeyValuePair<string, object>> config, string key, string defaultValue)
        {
            KeyValuePair<string, object>? item = config.FirstOrDefault(r => r.Key == key);
            if (item == null)
                return defaultValue;

            if (item.Value.Value == null)
                return string.Empty;

            return item.Value.Value.ToString();
        }

        #endregion
    }
}
