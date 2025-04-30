using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class Core_JobState: ICommand
    {
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
                ConsoleHelper.WriteLine($"ERROR : \"Job\" key required", addDate: false);
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("State"))
            {
                ConsoleHelper.WriteLine($"ERROR : \"State\" key required", addDate: false);
                Environment.Exit(1);
                return;
            }


            string jobKey = switches.Get("Job");
            string state = switches.Get("state");
            if (state != "pass" && state != "fail")
            {
                ConsoleHelper.WriteLine($"ERROR : \"State\" must be either \"pass\" or \"fail\".", addDate: false);
                Environment.Exit(1);
                return;
            }


            Job job = dataLayer.GetJobByKey(jobKey);
            if (job == null) 
            {
                ConsoleHelper.WriteLine($"Job key {jobKey} not valid", addDate: false);
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
                        ConsoleHelper.WriteLine("--logbuild value is not an integer", addDate: false);
                        Environment.Exit(1);
                        return;
                    }

                    Build logBuild = dataLayer.GetBuildById(switches.Get("logbuild"));
                    if (logBuild == null)
                    {
                        ConsoleHelper.WriteLine($"Could not find build with id {switches.Get("logbuild")}", addDate: false);
                        Environment.Exit(1);
                        return;
                    }

                    if (!logBuild.LogFetched)
                    {
                        ConsoleHelper.WriteLine($"build with id {switches.Get("logbuild")} has no log", addDate: false);
                        Environment.Exit(1);
                        return;
                    }

                    ConsoleHelper.WriteLine($"logpath for forced build will be set to path from build {logBuild.Id}.", addDate: false);
                }
                else 
                {
                    ConsoleHelper.WriteLine("--logbuild not set, ignoring. You can force an error log from an existing build with this switch.");
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

            ConsoleHelper.WriteLine($"Set job {job.Key} to {state}", addDate: false);
        }
    }
}
