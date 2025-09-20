using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI.Commands
{
    internal class IDataLayer_DeleteUser : ICommand
    {
        private readonly Logger _logger;

        public IDataLayer_DeleteUser(Logger logger) 
        {
            _logger = logger;
        }

        public string Describe()
        {
            return @"Deletes a user from database. This is meant for orphan cleanup";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            _logger.Status("Executing function IDataLayerPlugin.DeleteUser");
            if (!switches.Contains("key"))
            {
                _logger.Status($"ERROR : key required");
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
                _logger.Status(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
