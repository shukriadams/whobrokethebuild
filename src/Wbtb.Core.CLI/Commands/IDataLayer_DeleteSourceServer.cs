using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI.Commands
{
    internal class IDataLayer_DeleteSourceServer : ICommand
    {
        private readonly Logger _logger;

        public IDataLayer_DeleteSourceServer(Logger logger) 
        {
            _logger = logger;
        }

        public string Describe()
        {
            return @"Deletes a source server from database. This is meant for orphan cleanup";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            _logger.Status("Executing function IDataLayerPlugin.DeleteSourceServer");
            if (!switches.Contains("key"))
            {
                _logger.Status($"ERROR : key required");
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
                _logger.Status(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
