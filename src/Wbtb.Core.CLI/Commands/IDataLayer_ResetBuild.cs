using System;
using System.Runtime.InteropServices;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_ResetBuild : ICommand
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly Logger _logger;

        private readonly ConsoleCLIHelper _cliHelper;

        private readonly Cache _cache;

        #endregion

        #region CTORS

        public IDataLayer_ResetBuild(PluginProvider pluginProvider, Cache cache, Logger logger, ConsoleCLIHelper cliHelper)
        {
            _pluginProvider = pluginProvider;
            _logger = logger;
            _cliHelper = cliHelper;
            _cache = cache;
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
                _logger.Status($"ERROR : \"--build\" <buildid> required");
                Environment.Exit(1);
                return;
            }

            bool wipeCache = switches.Contains("cache");
            if (wipeCache)
                _logger.Status(this, "!wiping cached values where possible!");

            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            Build build = dataLayer.GetBuildByUniquePublicIdentifier(switches.Get("build"));
            if (build == null)
            {
                _logger.Status($"ERROR : \"--build\" id {build} does not point to a valid build");
                Environment.Exit(1);
                return;
            }

            bool hard = true;
            if (hard)
                _logger.Status("Performing hard reset");

            if (wipeCache) 
            {
                Job job = dataLayer.GetJobById(build.JobId);
                foreach (string logParserKey in job.LogParsers) 
                {
                    IPlugin parserPlugin = _pluginProvider.GetByKey(logParserKey);
                    _cache.Clear(parserPlugin.ContextPluginConfig.Manifest.Key, job, build);
                }
            }

            int affected = dataLayer.ResetBuild(build.Id, hard);

            // requeue build internally
            if (!hard)
                dataLayer.SaveDaemonTask(new DaemonTask
                {
                    BuildId = build.Id,
                    Stage = (int)ProcessStages.BuildEnd,
                    CreatedUtc = DateTime.UtcNow,
                    Src = this.GetType().Name
                });

            _logger.Status($"Build {build.Id} reset. {affected} records affected.");
        }

        #endregion
    }
}
