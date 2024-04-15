using System;
using System.IO;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class Core_ResolveRevisionFromLog : ICommand
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly ConsoleHelper _consoleHelper;

        #endregion

        #region CTORS

        public Core_ResolveRevisionFromLog(PluginProvider pluginProvider, ConsoleHelper consoleHelper)
        {
            _pluginProvider = pluginProvider;
            _consoleHelper = consoleHelper;
        }

        #endregion

        #region METHODS

        public string Describe()
        {
            return "Non-destructively parses a revision out of a build log. Requires regex set at build job level with \"RevisionAtBuildRegex\" property.";
        }

        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("build"))
            {
                ConsoleHelper.WriteLine($"ERROR : \"--build\" <buildid> required");
                Environment.Exit(1);
                return;
            }

            string buildid = switches.Get("build");

            SimpleDI di = new SimpleDI();
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Build build = dataLayer.GetBuildById(buildid);
            Configuration config = di.Resolve<Configuration>();

            if (build == null) 
            {
                ConsoleHelper.WriteLine($"ERROR : build {buildid} does not exist. Note this is a unique database id, not key/identifier from an external build srver.");
                Environment.Exit(1);
                return;
            }

            if (!build.LogFetched)
            {
                ConsoleHelper.WriteLine($"ERROR : build {buildid} has no log");
                Environment.Exit(1);
                return;
            }

            Job job = dataLayer.GetJobById(build.JobId);
            if (string.IsNullOrEmpty(job.RevisionAtBuildRegex)) 
            {
                ConsoleHelper.WriteLine($"ERROR : job {job.Name} for build {buildid} has no RevisionAtBuildRegex regex set.");
                Environment.Exit(1);
                return;
            }

            string log = File.ReadAllText(Build.GetLogPath(config, job, build));

            Match match = new Regex(job.RevisionAtBuildRegex, RegexOptions.IgnoreCase & RegexOptions.Multiline).Match(log);
            if (!match.Success || match.Groups.Count < 2)
            {
                ConsoleHelper.WriteLine("No revisions found");
                return;
            }

            ConsoleHelper.WriteLine("Found ");
            ConsoleHelper.WriteLine(match.Groups[1].Value);
        }

        #endregion
    }
}
