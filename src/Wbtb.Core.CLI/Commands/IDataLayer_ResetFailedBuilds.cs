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

        #endregion

        #region CTORS

        public IDataLayer_ResetFailedBuilds(PluginProvider pluginProvider)
        {
            _pluginProvider = pluginProvider;
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
                        ConsoleHelper.WriteLine($"ERROR : failing build id {build} not found, skipping", addDate: false);
                        continue;
                    }

                    // this is a delete where and will not fail on non-existent builds ....
                    // Note that hard reset will delete the build, which will cause a constraint error when trying to requeue its daemontask
                    dataLayer.ResetBuild(build.Id, false);

                    // requeue build internally
                    dataLayer.SaveDaemonTask(new DaemonTask
                    {
                        BuildId = build.Id,
                        Stage = 0, //"BuildEnd",
                        CreatedUtc = DateTime.UtcNow,
                        Src = this.GetType().Name
                    });
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteLine($"Error {ex} processing build id {buildId} ", addDate: false);
                }
            }

            ConsoleHelper.WriteLine($"{buildIds.Count()} builds reset.", addDate: false);
        }

        #endregion
    }
}
