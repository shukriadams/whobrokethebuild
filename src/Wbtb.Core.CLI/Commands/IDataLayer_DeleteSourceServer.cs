using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI.Commands
{
    internal class IDataLayer_DeleteSourceServer : ICommand
    {
        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            Console.WriteLine("Executing function IDataLayerPlugin.DeleteSourceServer");
            if (!switches.Contains("key"))
            {
                Console.WriteLine($"ERROR : key required");
                Environment.Exit(1);
                return;
            }


            string key = switches.Get("key");

            try
            {
                orphanRecordHelper.DeleteSourceServer(key);
            }
            catch (RecordNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
