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

            ConsoleHelper.WriteLine("Executing function IDataLayerPlugin.MergeSourceServers", addDate: false);
            if (!switches.Contains("from"))
            {
                ConsoleHelper.WriteLine($"ERROR : key \"from\" required", addDate: false);
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("to"))
            {
                ConsoleHelper.WriteLine($"ERROR : key \"to\" required", addDate: false);
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
                ConsoleHelper.WriteLine(ex.Message, addDate: false);
                Environment.Exit(1);
            }
        }
    }
}
