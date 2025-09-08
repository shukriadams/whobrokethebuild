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

        private readonly ConsoleHelper _consoleHelper;

        #endregion

        #region CTORS

        public IDataLayer_Reinitialize(PluginProvider pluginProvider, ConsoleHelper consoleHelper)
        {
            _pluginProvider = pluginProvider;
            _consoleHelper = consoleHelper;
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
                ConsoleHelper.WriteLine($"WARNING: This will delete _all_ data in backend, as well as destroy and recreate all tables etc. Please add --confirm switch to prove you mean this", addDate: false);
                Environment.Exit(1);
                return;
            }

            IDataPlugin data = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            data.DestroyDatastore();
            ConsoleHelper.WriteLine("Datastore destroyed", addDate: false);
            data.InitializeDatastore();
            ConsoleHelper.WriteLine("Datastore initialized", addDate: false);
        }

        #endregion
    }
}
