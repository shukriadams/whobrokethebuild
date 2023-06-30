using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI.Lib
{
    internal class ConsoleHelper
    {
        private readonly IDataPlugin _datalayer;
        public ConsoleHelper(PluginProvider pluginProvider) 
        {
            _datalayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
        }
        public void PrintJobs() 
        {
            IEnumerable<Job> jobs = _datalayer.GetJobs();
            if (jobs.Any())
            {
                Console.WriteLine("Existing jobs are : ");
                foreach (Job existingJob in jobs)
                    Console.WriteLine($"Name:{existingJob.Name} id: {existingJob.Id}");
            }

        }
    }
}
