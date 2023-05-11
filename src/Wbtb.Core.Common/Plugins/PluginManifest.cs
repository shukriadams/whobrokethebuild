namespace Wbtb.Core.Common
{
    public class PluginManifest
    {
        /// <summary>
        /// Unique global identifier of plugin. Official WBTB plugins will start with "WBTB.", externals will start with "Community." etc
        /// </summary>
        public string Key {get;set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Runtime used to execute plugin. Must be available at commnd line on host system. Will be parsed down to enumerator Runtimes.
        /// </summary>
        public string Runtime { get; set; }

        /// <summary>
        /// Enum-parsed version of Runtime.
        /// </summary>
        public Runtimes RuntimeParsed { get; set; }

        /// <summary>
        /// This is dev-only.
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Main { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Version of WBTB common API that plugin expects. This is checked on app startup
        /// </summary>
        public string APIVersion { get; set; }

        /// <summary>
        /// Pl
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// Interface name in namespace+shortname form
        /// </summary>
        public string Interface { get; set; }

        /// <summary>
        /// Used on strongly-typed dev systems only.
        /// </summary>
        public string Concrete { get; set; }
    }
}
