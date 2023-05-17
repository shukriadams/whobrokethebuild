using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayerPlugin_ResetJob : ICommand
    {
        public void Process(CommandLineSwitches switches) 
        {
            if (!switches.Contains("job"))
            {
                Console.WriteLine($"ERROR : \"job\" key required");
                Environment.Exit(1);
                return;
            }

            string jobkey = switches.Get("job");
            bool hard = switches.Contains("hard");

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataLayerPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            Job job = dataLayer.GetJobByKey(jobkey);
            if (job == null)
            {
                Console.WriteLine($"ERROR : \"job\" key {jobkey} does not point to a valid job");
                IEnumerable<Job> jobs = dataLayer.GetJobs();
                if (jobs.Any()) 
                {
                    Console.WriteLine("Existing job keys are : ");
                    foreach (Job existingJob in jobs) 
                    {
                        Console.WriteLine($"{existingJob.Key}");
                    }
                }
                Environment.Exit(1);    
                return;
            }

            if (hard)
                Console.WriteLine("Performing hard reset");
            else
                Console.WriteLine("Performing reset - to force delete all child records under job use --hard switch");

            int deleted = dataLayer.ResetJob(job.Id, hard);

            Console.Write($"Job reset. {deleted} records deleted.");
        }
    }
}
