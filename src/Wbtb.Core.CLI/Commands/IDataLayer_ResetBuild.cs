using System;
using Wbtb.Core.CLI.Lib;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_ResetBuild : ICommand
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly ConsoleHelper _consoleHelper;

        #endregion

        #region CTORS

        public IDataLayer_ResetBuild(PluginProvider pluginProvider, ConsoleHelper consoleHelper)
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
                Console.WriteLine($"ERROR : \"--build\" <buildid> required");
                Environment.Exit(1);
                return;
            }

            string buildId = switches.Get("build");
            bool hard = switches.Contains("hard");

            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            Build build = dataLayer.GetBuildById(buildId);
            if (build == null)
            {
                Console.WriteLine($"ERROR : \"--build\" id {build} does not point to a valid build");
                _consoleHelper.PrintJobs();
                Environment.Exit(1);
                return;
            }

            if (hard)
                Console.WriteLine("Performing hard reset");
            else
                Console.WriteLine("Performing reset - to force delete all child records under build use --hard switch");

            int deleted = dataLayer.ResetBuild(build.Id, hard);
            dataLayer.SaveDaemonTask(new DaemonTask
            {
                BuildId = build.Id,
                Stage = 0, //"BuildEnd",
                CreatedUtc = DateTime.UtcNow,
                Src = this.GetType().Name
            });

            Console.Write($"Build {build.Id} reset. {deleted} records deleted.");
        }

        #endregion
    }
}
