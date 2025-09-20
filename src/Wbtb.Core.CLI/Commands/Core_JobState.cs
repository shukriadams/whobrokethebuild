using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class Core_JobState: ICommand
    {
        private readonly Logger _logger;

        public Core_JobState(Logger logger) 
        {
            _logger = logger;
        }

        public string Describe() 
        {
            return @"Sets a job state to passing or failing. Use this to simulate build breaks/fixes. Failing builds can also be populated with logs from a known build, to test log handling.";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetDistinct<IDataPlugin>();

            if (!switches.Contains("Job"))
            {
                _logger.Status($"ERROR : \"Job\" key required");
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("State"))
            {
                _logger.Status($"ERROR : \"State\" key required");
                Environment.Exit(1);
                return;
            }


            string jobKey = switches.Get("Job");
            string state = switches.Get("state");
            if (state != "pass" && state != "fail")
            {
                _logger.Status($"ERROR : \"State\" must be either \"pass\" or \"fail\".");
                Environment.Exit(1);
                return;
            }


            Job job = dataLayer.GetJobByKey(jobKey);
            if (job == null) 
            {
                _logger.Status($"Job key {jobKey} not valid");
                Environment.Exit(1);
                return;
            }

            int key = 0;
            while (true) 
            {
                key++;
                if (dataLayer.GetBuildByKey(job.Id, key.ToString()) == null)
                    break;
            }

            string logPath = null;

            if (state == "fail") 
            {
                if (switches.Contains("logbuild"))
                {
                    int buildId = 0;
                    if (!int.TryParse(switches.Get("logbuild"), out buildId)) 
                    {
                        _logger.Status("--logbuild value is not an integer");
                        Environment.Exit(1);
                        return;
                    }

                    Build logBuild = dataLayer.GetBuildById(switches.Get("logbuild"));
                    if (logBuild == null)
                    {
                        _logger.Status($"Could not find build with id {switches.Get("logbuild")}");
                        Environment.Exit(1);
                        return;
                    }

                    if (!logBuild.LogFetched)
                    {
                        _logger.Status($"build with id {switches.Get("logbuild")} has no log");
                        Environment.Exit(1);
                        return;
                    }

                    _logger.Status($"logpath for forced build will be set to path from build {logBuild.Id}.");
                }
                else 
                {
                    _logger.Status("--logbuild not set, ignoring. You can force an error log from an existing build with this switch.");
                }
            }

            Build build = new Build
            {
                JobId = job.Id,
                Key = key.ToString(),
                StartedUtc = DateTime.UtcNow,
                EndedUtc = DateTime.UtcNow,
                LogFetched = true,
                Status = state == "fail" ? BuildStatus.Failed : BuildStatus.Passed,
            };
            build.SetUniquePublicIdentifier(job);

            dataLayer.SaveBuild(build);

            _logger.Status($"Set job {job.Key} to {state}");
        }
    }
}
