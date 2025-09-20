using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_MergeSourceServers : ICommand
    {
        private readonly Logger _logger;

        public IDataLayer_MergeSourceServers(Logger logger) 
        {
            _logger = logger;
        }

        public string Describe()
        {
            return @"Nigrate child records of an orphan source server to another source server. The orphan source server is deleted in the process.";
        }

        public void Process(CommandLineSwitches switches) 
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            _logger.Status("Executing function IDataLayerPlugin.MergeSourceServers");
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

            string fromSourceServerKey = switches.Get("from");
            string toSourceServerKey = switches.Get("to");

            try
            {
                orphanRecordHelper.MergeSourceServers(fromSourceServerKey, toSourceServerKey);
            }
            catch (RecordNotFoundException ex)
            {
                _logger.Status(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
