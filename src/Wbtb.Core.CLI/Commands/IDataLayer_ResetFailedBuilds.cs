using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_ResetFailedBuilds : ICommand
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly Logger _logger;

        #endregion

        #region CTORS

        public IDataLayer_ResetFailedBuilds(PluginProvider pluginProvider, Logger logger)
        {
            _pluginProvider = pluginProvider;
            _logger = logger;
        }

        #endregion

        #region METHODS

        public string Describe()
        {
            return @"Resets all failed builds.";
        }

        public void Process(CommandLineSwitches switches)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            IEnumerable<string> buildIds = dataLayer.GetFailingDaemonTasksBuildIds();
            foreach (string buildId in buildIds) 
            {
                try
                {
                    Build build = dataLayer.GetBuildById(buildId);
                    if (build == null)
                    {
                        _logger.Status($"ERROR : failing build id {build} not found, skipping");
                        continue;
                    }

                    // this is a delete where and will not fail on non-existent builds ....
                    // Note that hard reset will delete the build, which will cause a constraint error when trying to requeue its daemontask
                    dataLayer.ResetBuild(build.Id, false);

                    // requeue build internally
                    dataLayer.SaveDaemonTask(new DaemonTask
                    {
                        BuildId = build.Id,
                        Stage = (int)ProcessStages.BuildEnd,
                        CreatedUtc = DateTime.UtcNow,
                        Src = this.GetType().Name
                    });
                }
                catch (Exception ex)
                {
                    _logger.Status($"Error {ex} processing build id {buildId} ");
                }
            }

            _logger.Status($"{buildIds.Count()} builds reset.");
        }

        #endregion
    }
}
