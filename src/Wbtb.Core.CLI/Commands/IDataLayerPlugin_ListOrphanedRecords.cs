using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayerPlugin_ListOrphanedRecords : ICommand
    {
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
