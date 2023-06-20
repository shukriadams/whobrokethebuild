using Microsoft.Extensions.Logging;
using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildUpdateDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly BuildLevelPluginHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;
        #endregion

        #region CTORS

        public BuildUpdateDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            _di = new SimpleDI();
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>();
            _buildLevelPluginHelper = _di.Resolve<BuildLevelPluginHelper>();
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _processRunner.Start(new DaemonWork(this.Work), tickInterval);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _processRunner.Dispose();
        }

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        private void Work()
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            // start daemons - this should be folded into start
            foreach (BuildServer cfgbuildServer in _config.BuildServers)
            {
                BuildServer buildServer = dataLayer.GetBuildServerByKey(cfgbuildServer.Key);
                IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

                int count = 100;
                if (buildServer.ImportCount.HasValue)
                    count = buildServer.ImportCount.Value;

                if (!reach.Reachable)
                {
                    _log.LogError($"Buildserver {buildServer.Key} not reachable, job import aborted {reach.Error}{reach.Exception}");
                    return;
                }

                foreach (Job job in buildServer.Jobs)
                {
                    try
                    {
                        Job thisjob = dataLayer.GetJobByKey(job.Key);
                        if (thisjob.ImportCount.HasValue)
                            count = thisjob.ImportCount.Value;

                        BuildImportSummary importSummary = new BuildImportSummary{ }; //;buildServerPlugin.ImportBuilds(thisjob, count);

                        // fires when a build record gets its end date, there is no guarantee of build log state or 
                        // other processes that would still have to be carried out
                        foreach (Build build in importSummary.Ended)
                            _buildLevelPluginHelper.InvokeEvents("OnBuildEnd", job.OnBuildEnd, build);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"Unexpected error trying to import builds for \"{job.Key}\" from buildserver \"{buildServer.Key}\" : {ex}");
                    }
                }
            }
        }

        #endregion
    }
}
