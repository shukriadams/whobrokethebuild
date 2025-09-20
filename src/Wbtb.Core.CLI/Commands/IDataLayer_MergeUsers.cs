using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI.Commands
{
    internal class IDataLayer_MergeUsers : ICommand
    {
        private readonly Logger _logger;

        public IDataLayer_MergeUsers(Logger logger) 
        { 
            _logger = logger;
        }

        public string Describe()
        {
            return @"Nigrate child records of an orphan user to another user. The orphan user is deleted in the process.";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            _logger.Status("Executing function IDataLayerPlugin.MergeUsers");

            if (!switches.Contains("from"))
            {
                _logger.Status($"ERROR : key \"from\" required");
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("to"))
            {
                _logger.Status($"ERROR : key \"to\" required");
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
                _logger.Status(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
