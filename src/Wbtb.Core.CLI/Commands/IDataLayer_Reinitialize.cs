using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{

    /// <summary>
    /// 
    /// </summary>
    internal class IDataLayer_Reinitialize : ICommand
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly Logger _logger;

        #endregion

        #region CTORS

        public IDataLayer_Reinitialize(PluginProvider pluginProvider, Logger logger)
        {
            _pluginProvider = pluginProvider;
            _logger = logger;
        }

        #endregion

        #region METHODS

        public string Describe()
        {
            return @"Deletes and rebuilds entire Wbtb database (all data, all tables, everything). This is a destructive operation. This operation is necessary to apply schema changes.";
        }


        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("confirm"))
            {
                _logger.Status($"WARNING: This will delete _all_ data in backend, as well as destroy and recreate all tables etc. Please add --confirm switch to prove you mean this");
                Environment.Exit(1);
                return;
            }

            IDataPlugin data = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            data.DestroyDatastore();
            _logger.Status("Datastore destroyed");
            data.InitializeDatastore();
            _logger.Status("Datastore initialized");
        }

        #endregion
    }
}
