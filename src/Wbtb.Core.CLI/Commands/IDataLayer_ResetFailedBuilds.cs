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

        private readonly ConsoleCLIHelper _consoleHelper;

        #endregion

        #region CTORS

        public IDataLayer_ResetFailedBuilds(PluginProvider pluginProvider, ConsoleCLIHelper consoleHelper)
        {
            _pluginProvider = pluginProvider;
            _consoleHelper = consoleHelper;
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
                Build build = dataLayer.GetBuildById(buildId);
                if (build == null)
                {
                    ConsoleHelper.WriteLine($"ERROR : expected build id {build} not found, skipping");
                    Environment.Exit(1);
                    return;
                }

                dataLayer.ResetBuild(build.Id, true);

                // requeue build internally
                dataLayer.SaveDaemonTask(new DaemonTask
                {
                    BuildId = build.Id,
                    Stage = 0, //"BuildEnd",
                    CreatedUtc = DateTime.UtcNow,
                    Src = this.GetType().Name
                });
            }

            Console.Write($"{buildIds.Count()} builds reset.");
        }

        #endregion
    }
}
