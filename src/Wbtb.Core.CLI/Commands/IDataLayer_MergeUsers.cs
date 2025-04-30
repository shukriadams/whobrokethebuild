using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI.Commands
{
    internal class IDataLayer_MergeUsers : ICommand
    {
        public string Describe()
        {
            return @"Nigrate child records of an orphan user to another user. The orphan user is deleted in the process.";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            ConsoleHelper.WriteLine("Executing function IDataLayerPlugin.MergeUsers", addDate: false);

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

            string fromUserKey = switches.Get("from");
            string toUserKey = switches.Get("to");

            try
            {
                orphanRecordHelper.MergeUsers(fromUserKey, toUserKey);
            }
            catch (RecordNotFoundException ex)
            {
                ConsoleHelper.WriteLine(ex.Message, addDate: false);
                Environment.Exit(1);
            }
        }
    }
}
