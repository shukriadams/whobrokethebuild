using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class ConsoleCLIHelper
    {
        private readonly IDataPlugin _datalayer;

        private readonly Logger _logger;

        public ConsoleCLIHelper(PluginProvider pluginProvider, Logger logger) 
        {
            _datalayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            _logger = logger;
        }

        public void PrintJobs() 
        {
            IEnumerable<Job> jobs = _datalayer.GetJobs();
            if (jobs.Any())
            {
                _logger.Status("Existing jobs are : ");
                foreach (Job existingJob in jobs)
                    _logger.Status($"Name:{existingJob.Name} key: {existingJob.Key}");
            }
        }
    }
}
