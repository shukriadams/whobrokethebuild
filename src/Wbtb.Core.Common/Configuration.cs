using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// config is loaded from multiple sources.
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
        /// 
        /// </summary>
        public bool FailOnOrphans { get; set; }

        /// <summary>
        /// Global default timer for daemons (seconds, will be converted to ms)
        /// </summary>
        public int DaemonInterval { get; set; }

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
        /// Bytes. Imposes limit on log files for parsing.
        /// </summary>
        public long MaxReadableRawLogSize { get; set; }

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
        /// 
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
            this.MaxReadableRawLogSize = 30000000;

            this.LiveConsoleSize = 500;
            this.DaemonInterval = 60;
            this.UnprocessedBuildProcessorLimit = 50;
            this.Port = 5000;
            this.EnabledDaemons = true;
            this.EnabledSockets = true;
            this.MaxThreads = 2;

            this.Plugins = new List<PluginConfig>();
            this.BuildServers = new List<BuildServer>();
            this.SourceServers = new List<SourceServer>();
            this.Users = new List<User>();
            this.Groups = new List<Group>();
        }

        #endregion
    }
}
