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
            IDataLayerPlugin dataLayer = pluginProvider.GetDistinct<IDataLayerPlugin>();

            if (!switches.Contains("Job"))
            {
                Console.WriteLine($"ERROR : \"Job\" key required");
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("State"))
            {
                Console.WriteLine($"ERROR : \"State\" key required");
                Environment.Exit(1);
                return;
            }


            string jobKey = switches.Get("Job");
            string state = switches.Get("state");
            if (state != "pass" && state != "fail")
            {
                Console.WriteLine($"ERROR : \"State\" must be either \"pass\" or \"fail\".");
                Environment.Exit(1);
                return;
            }


            Job job = dataLayer.GetJobByKey(jobKey);
            if (job == null) 
            {
                Console.WriteLine($"Job key {jobKey} not valid");
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
                        Console.WriteLine("--logbuild value is not an integer");
                        Environment.Exit(1);
                        return;
                    }

                    Build logBuild = dataLayer.GetBuildById(switches.Get("logbuild"));
                    if (logBuild == null)
                    {
                        Console.WriteLine($"Could not find build with id {switches.Get("logbuild")}");
                        Environment.Exit(1);
                        return;
                    }

                    if (logBuild.LogPath == null)
                    {
                        Console.WriteLine($"build with id {switches.Get("logbuild")} has no log");
                        Environment.Exit(1);
                        return;
                    }

                    logPath = logBuild.LogPath;
                    Console.WriteLine($"logpath for forced build will be set to path from build {logBuild.Id}.");
                }
                else 
                {
                    Console.WriteLine("--logbuild not set, ignoring. You can force an error log from an existing build with this switch.");
                }
            }

            dataLayer.SaveBuild(new Build
            {
                JobId = job.Id,
                Identifier = key.ToString(),
                StartedUtc = DateTime.UtcNow,
                EndedUtc = DateTime.UtcNow,
                LogPath = logPath,
                Status = state == "fail" ? BuildStatus.Failed : BuildStatus.Passed,
            });

            Console.WriteLine($"Set job {job.Key} to {state}");
        }
    }
}
