namespace Wbtb.Core.Common
{
    public class PluginArgs
    {
        public string FunctionName { get;set;}

        /// <summary>
        /// Unique Id of plugin as defined in config.yml
        /// </summary>
        public string pluginKey { get; set; }

        public PluginFunctionParameter[] Arguments { get; set; }
    }
}
