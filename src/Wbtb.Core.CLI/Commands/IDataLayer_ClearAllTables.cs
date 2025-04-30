using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_ClearAllTables : ICommand
    {
        public string Describe()
        {
            return @"Deletes all data in all tables. This is a destructive reset";
        }

        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("confirm"))
            {
                ConsoleHelper.WriteLine($"WARNING: This will delete _all_ data in backend. Please add --confirm switch to prove you mean this", addDate: false);
                Environment.Exit(1);
                return;
            }

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin dataLayerPlugin = pluginProvider.GetFirstForInterface<IDataPlugin>();

            int count = dataLayerPlugin.ClearAllTables();
            ConsoleHelper.WriteLine($"Tables cleared, {count} records removed.", addDate: false);
        }
    }
}
