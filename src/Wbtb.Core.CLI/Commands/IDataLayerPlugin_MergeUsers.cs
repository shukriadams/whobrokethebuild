using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI.Commands
{
    internal class IDataLayerPlugin_MergeUsers : ICommand
    {
        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            Console.WriteLine("Executing function IDataLayerPlugin.MergeUsers");
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

            string fromUserKey = switches.Get("from");
            string toUserKey = switches.Get("to");

            try
            {
                orphanRecordHelper.MergeUsers(fromUserKey, toUserKey);
            }
            catch (RecordNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
