using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_DeleteJob : ICommand
    {
        private readonly Logger _logger;

        public IDataLayer_DeleteJob(Logger logger) 
        {
            _logger = logger;
        }

        public string Describe()
        {
            return @"Deletes a job from database. This is meant for orphan cleanup";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            if (!switches.Contains("job"))
            {
                _logger.Status($"ERROR : --job required");
                Environment.Exit(1);
                return;
            }

            string jobKey = switches.Get("job");

            try
            {
                orphanRecordHelper.DeleteJob(jobKey);
            }
            catch (RecordNotFoundException ex)
            {
                _logger.Status(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
