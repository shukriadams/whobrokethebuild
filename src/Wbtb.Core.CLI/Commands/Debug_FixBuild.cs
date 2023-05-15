using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class Debug_FixBuild :ICommand
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
            string jobKey = switches.Get("Job");

            Job job = dataLayer.GetJobByKey(jobKey);
            if (job == null)
            {
                Console.WriteLine($"Job key {jobKey} not valid");
                Environment.Exit(1);
                return;
            }

            dataLayer.SaveBuild(new Build
            {
                JobId = job.Id,
                Identifier = Guid.NewGuid().ToString(),
                StartedUtc = DateTime.UtcNow,
                EndedUtc = DateTime.UtcNow,
                Status = BuildStatus.Passed,
            });

            Console.WriteLine($"Fixed job {job.Key} - order has been restored");
        }
    }
}
