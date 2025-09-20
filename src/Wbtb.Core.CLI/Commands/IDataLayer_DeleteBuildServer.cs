using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI.Commands
{
    internal class IDataLayer_DeleteBuildServer : ICommand
    {
        private readonly Logger _logger;

        public IDataLayer_DeleteBuildServer(Logger logger) 
        {
            _logger = logger;
        }

        public string Describe()
        {
            return @"Deletes a build server from database. This is meant for orphan cleanup";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            _logger.Status("Executing function IDataLayerPlugin.DeleteBuildServer");
            if (!switches.Contains("key"))
            {
                _logger.Status($"ERROR : key required");
                Environment.Exit(1);
                return;
            }

            string key = switches.Get("key");

            try
            {
                orphanRecordHelper.DeleteBuildServer(key);
            }
            catch (RecordNotFoundException ex)
            {
                _logger.Status(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
