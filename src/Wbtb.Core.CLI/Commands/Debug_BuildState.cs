using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class Debug_BuildState: ICommand
    {
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

            dataLayer.SaveBuild(new Build
            {
                JobId = job.Id,
                Identifier = key.ToString(),
                StartedUtc = DateTime.UtcNow,
                EndedUtc = DateTime.UtcNow,
                Status = state == "fail" ? BuildStatus.Failed : BuildStatus.Passed,
            });

            Console.WriteLine($"Set job {job.Key} to {state}");
        }
    }
}
