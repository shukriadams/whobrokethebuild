using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_ResetBuild : ICommand
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly ConsoleCLIHelper _consoleHelper;

        #endregion

        #region CTORS

        public IDataLayer_ResetBuild(PluginProvider pluginProvider, ConsoleCLIHelper consoleHelper)
        {
            _pluginProvider = pluginProvider;
            _consoleHelper = consoleHelper;
        }

        #endregion

        #region METHODS

        public string Describe()
        {
            return @"Resets a single build. Practical for debugging.";
        }

        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("build"))
            {
                ConsoleHelper.WriteLine($"ERROR : \"--build\" <buildid> required", addDate: false);
                Environment.Exit(1);
                return;
            }

            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            Build build = dataLayer.GetBuildByUniquePublicIdentifier(switches.Get("build"));
            if (build == null)
            {
                ConsoleHelper.WriteLine($"ERROR : \"--build\" id {build} does not point to a valid build", addDate: false);
                _consoleHelper.PrintJobs();
                Environment.Exit(1);
                return;
            }

            bool hard = true;
            if (hard)
                ConsoleHelper.WriteLine("Performing hard reset", addDate: false);

            int affected = dataLayer.ResetBuild(build.Id, hard);

            // requeue build internally
            if (!hard)
                dataLayer.SaveDaemonTask(new DaemonTask
                {
                    BuildId = build.Id,
                    Stage = 0, //"BuildEnd",
                    CreatedUtc = DateTime.UtcNow,
                    Src = this.GetType().Name
                });

            ConsoleHelper.WriteLine($"Build {build.Id} reset. {affected} records affected.", addDate: false);
        }

        #endregion
    }
}
