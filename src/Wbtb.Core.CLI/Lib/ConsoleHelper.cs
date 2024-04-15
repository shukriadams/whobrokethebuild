using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class ConsoleCLIHelper
    {
        private readonly IDataPlugin _datalayer;
        
        public ConsoleCLIHelper(PluginProvider pluginProvider) 
        {
            _datalayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
        }

        public void PrintJobs() 
        {
            IEnumerable<Job> jobs = _datalayer.GetJobs();
            if (jobs.Any())
            {
                ConsoleHelper.WriteLine("Existing jobs are : ");
                foreach (Job existingJob in jobs)
                    ConsoleHelper.WriteLine($"Name:{existingJob.Name} id: {existingJob.Id}");
            }

        }
    }
}
