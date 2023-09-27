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
        /// 
        /// </summary>
        public string LogPath { get; set; }

        #endregion

        #region CTORS

        public ConfigurationBasic()
        {
            this.MessageQueuePort = 5001;
            this.PersistCalls = true;

            this.PersistCalls = EnvironmentVariableHelper.GetBool("WBTB_PERSISTCALLS", false);
            this.ProxyMode = EnvironmentVariableHelper.GetString("WBTB_PROXYMODE", "default");
            this.ConfigPath = EnvironmentVariableHelper.GetString(Constants.ENV_VAR_CONFIG_PATH, Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.yml"));
            this.DataRootPath = EnvironmentVariableHelper.GetString("WBTB_DATA_ROOT", Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Data")); ;
            this.LogPath = EnvironmentVariableHelper.GetString("WBTB_LOG_PATH", Path.Join(this.DataRootPath, "logs", "log.txt"));

            this.BuildLogsDirectory = Path.Join(this.DataRootPath, "BuildLogs");
            this.PluginDataPersistDirectory = Path.Join(this.DataRootPath, "PluginData");
            this.PluginsWorkingDirectory = Path.Join(this.DataRootPath, "PluginsWorking");
        }

        #endregion
    }
}
