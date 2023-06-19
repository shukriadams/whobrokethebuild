using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_MergeSourceServers : ICommand
    {
        public string Describe()
        {
            return @"Nigrate child records of an orphan source server to another source server. The orphan source server is deleted in the process.";
        }

        public void Process(CommandLineSwitches switches) 
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            Console.WriteLine("Executing function IDataLayerPlugin.MergeSourceServers");
            if (!switches.Contains("from"))
            {
                Console.WriteLine($"ERROR : key \"from\" required");
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("to"))
            {
                Console.WriteLine($"ERROR : key \"to\" required");
                Environment.Exit(1);
                return;
            }

            string fromSourceServerKey = switches.Get("from");
            string toSourceServerKey = switches.Get("to");

            try
            {
                orphanRecordHelper.MergeSourceServers(fromSourceServerKey, toSourceServerKey);
            }
            catch (RecordNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
