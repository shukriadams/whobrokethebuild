using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI.Commands
{
    internal class IDataLayer_DeleteUser : ICommand
    {
        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            Console.WriteLine("Executing function IDataLayerPlugin.DeleteUser");
            if (!switches.Contains("key"))
            {
                Console.WriteLine($"ERROR : key required");
                Environment.Exit(1);
                return;
            }

            string key = switches.Get("key");

            try
            {
                orphanRecordHelper.DeleteUser(key);
            }
            catch (RecordNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
