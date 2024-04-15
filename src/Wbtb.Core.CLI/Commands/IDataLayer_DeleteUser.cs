using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI.Commands
{
    internal class IDataLayer_DeleteUser : ICommand
    {
        public string Describe()
        {
            return @"Deletes a user from database. This is meant for orphan cleanup";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            ConsoleHelper.WriteLine("Executing function IDataLayerPlugin.DeleteUser");
            if (!switches.Contains("key"))
            {
                ConsoleHelper.WriteLine($"ERROR : key required");
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
                ConsoleHelper.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
