using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_ListOrphans : ICommand
    {
        public string Describe()
        {
            return @"Lists orphan records in database. Orphans occur when record keys are changed abruptly without providing key renaming options in config.";
        }

        public void Process(CommandLineSwitches switches) 
        {
            SimpleDI di = new SimpleDI();
            ConfigurationBuilder configurationBuilder = di.Resolve<ConfigurationBuilder>();

            Console.WriteLine("Executing function IDataLayerPlugin.ListOrphanedRecords");
            IEnumerable<string> orphans = configurationBuilder.FindOrphans();
            foreach (string orphan in orphans)
                Console.WriteLine(orphan);
        }
    }
}
