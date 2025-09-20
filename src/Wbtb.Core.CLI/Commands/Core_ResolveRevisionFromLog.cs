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

        private readonly Logger _logger;

        #endregion

        #region CTORS

        public Core_ResolveRevisionFromLog(PluginProvider pluginProvider, Logger logger)
        {
            _pluginProvider = pluginProvider;
            _logger = logger;
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
                _logger.Status($"ERROR : \"--build\" <buildid> required");
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
                _logger.Status($"ERROR : build {buildid} does not exist. Note this is a unique database id, not key/identifier from an external build srver.");
                Environment.Exit(1);
                return;
            }

            if (!build.LogFetched)
            {
                _logger.Status($"ERROR : build {buildid} has no log");
                Environment.Exit(1);
                return;
            }

            Job job = dataLayer.GetJobById(build.JobId);
            if (string.IsNullOrEmpty(job.RevisionAtBuildRegex)) 
            {
                _logger.Status($"ERROR : job {job.Name} for build {buildid} has no RevisionAtBuildRegex regex set.");
                Environment.Exit(1);
                return;
            }

            string log = File.ReadAllText(Build.GetLogPath(config, job, build));

            Match match = new Regex(job.RevisionAtBuildRegex, RegexOptions.IgnoreCase & RegexOptions.Multiline).Match(log);
            if (!match.Success || match.Groups.Count < 2)
            {
                _logger.Status("No revisions found");
                return;
            }

            _logger.Status("Found");
            _logger.Status(match.Groups[1].Value);
        }

        #endregion
    }
}
