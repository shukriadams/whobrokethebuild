using System.IO;
using System;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Simple configuration required for baseline functionality, including for plugins. Config is not checked as thoroughly as main config,
    /// config errors will not cause application start to halt, but will rather revert to reasonable defaults.
    /// </summary>
    public class ConfigurationBasic
    {
        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public int MessageQueuePort { get;set; }

        /// <summary>
        /// 
        /// </summary>
        public bool PersistCalls { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ProxyMode { get;set;}

        /// <summary>
        /// Root dir where all WBTB is persisted. This dir contains other named directories based on specific nature
        /// </summary>
        public string DataRootPath { get; set; }

        /// <summary>
        /// Directory plugins can persist raw data to. Each plugin shoudl write to a sub dir names after itself
        /// </summary>
        public string PluginDataPersistDirectory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string BuildLogsDirectory { get; set; }

        /// <summary>
        /// Internal path plugins are copied to. This directory is owned by WBTB. Do not store plugin data in here.
        /// </summary>
        public string PluginsWorkingDirectory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ConfigPath { get; set; }

        /// <summary>
        /// Path WBTB log most messages to. Logs from plugins and core logic will be written here.
        /// </summary>
        public string LogPath { get; set; }

        /// <summary>
        /// Path WBTB stores cached metrics files. File are generated periodically, then served by the metrics API.
        /// </summary>
        public string MetricsPath { get; set; }

        /// <summary>
        /// Path dotnet spams its Windows-style trash logs to. Segregated because impossible to regulate.
        /// </summary>
        public string DotNetLogPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string GitConfigUrl { get; set; }

        /// <summary>
        /// Normally enabled in dev environments. If false, plugins are validated only after config change.
        /// </summary>
        public bool ForcePluginValidation { get; set; }

        #endregion

        #region CTORS

        public ConfigurationBasic()
        {
            this.MessageQueuePort = 5001;
            this.PersistCalls = true;

            this.GitConfigUrl = EnvironmentVariableHelper.GetString("WBTB_GIT_CONFIG_REPO_URL");
            this.PersistCalls = EnvironmentVariableHelper.GetBool("WBTB_PERSISTCALLS", false);
            this.ForcePluginValidation = EnvironmentVariableHelper.GetBool("WBTB_FORCE_PLUGIN_VALIDATION", false);
            this.ProxyMode = EnvironmentVariableHelper.GetString("WBTB_PROXYMODE", "default");
            this.ConfigPath = EnvironmentVariableHelper.GetString(Constants.ENV_VAR_CONFIG_PATH, Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.yml"));
            this.DataRootPath = EnvironmentVariableHelper.GetString("WBTB_DATA_ROOT", Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Data")); ;
            this.LogPath = EnvironmentVariableHelper.GetString("WBTB_LOG_PATH", Path.Join(this.DataRootPath, "logs", "log.txt"));
            this.DotNetLogPath = EnvironmentVariableHelper.GetString("WBTB_DOTNETLOG_PATH", Path.Join(this.DataRootPath, "logs", "log-sys.txt"));
            this.MetricsPath = EnvironmentVariableHelper.GetString("WBTB_METRICS_PATH", Path.Join(this.DataRootPath, "Metrics"));

            this.BuildLogsDirectory = Path.Join(this.DataRootPath, "BuildLogs");
            this.PluginDataPersistDirectory = Path.Join(this.DataRootPath, "PluginData");
            this.PluginsWorkingDirectory = Path.Join(this.DataRootPath, "PluginsWorking");
        }

        /// <summary>
        /// Run this as early in app lifecycle as possible, ideally before any code has a chance to create instances of this class.
        /// </summary>
        /// <exception cref="ConfigurationException"></exception>
        public static void ValidateAndOverrideDefaults() 
        {
            string localGitUrlFile = "./.giturl";
            if (File.Exists(localGitUrlFile))
            {
                string urlCheck = File.ReadAllText(localGitUrlFile);
                if (string.IsNullOrEmpty(urlCheck))
                    throw new ConfigurationException("GIT-CONFIG : ./.giturl override file exists, but is empty. File should contain git url to sync config from.");


                Console.WriteLine($"Git url file found at ${localGitUrlFile}, will use this to fetch config.");

                Environment.SetEnvironmentVariable("WBTB_GIT_CONFIG_REPO_URL", urlCheck);
            }

            string gitConfigUrl = EnvironmentVariableHelper.GetString("WBTB_GIT_CONFIG_REPO_URL");
            
            if (!string.IsNullOrEmpty(gitConfigUrl)) 
            {
                string dataRootPath = EnvironmentVariableHelper.GetString("WBTB_DATA_ROOT", Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Data")); ;
                string gitConfigLocalPath = Path.Join(dataRootPath, "ConfigCheckout", "config.yml");

                Console.WriteLine($"Git config url specified, force setting config path to \"{gitConfigLocalPath}\", assuming a config file will be found at that path.");

                Environment.SetEnvironmentVariable(Constants.ENV_VAR_CONFIG_PATH, gitConfigLocalPath);
            }
        }

        #endregion
    }
}
