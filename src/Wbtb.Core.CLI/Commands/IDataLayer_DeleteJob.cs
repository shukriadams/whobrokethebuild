using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_DeleteJob : ICommand
    {
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
                Console.WriteLine($"ERROR : --job required");
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
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
