namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(BlamePluginProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface IBlamePlugin : IPlugin
    {
        /// <summary>
        /// Processes a build to try to determine causes of build failure. Assumes that all data gathering 
        /// for build is complete : logs are parsed, build involvements calculated, users and revisions resolved.
        /// Blame is written to build involvement records on build.
        /// </summary>
        /// <param name="build"></param>
        void BlameBuildFailure(Build failingBuild);
    }
}
