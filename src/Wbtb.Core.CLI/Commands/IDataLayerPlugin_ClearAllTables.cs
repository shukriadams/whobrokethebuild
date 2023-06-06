using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayerPlugin_ClearAllTables : ICommand
    {
        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("confirm"))
            {
                Console.WriteLine($"WARNING: This will delete _all_ data in backend. Please add --confirm switch to prove you mean this");
                Environment.Exit(1);
                return;
            }

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataLayerPlugin dataLayerPlugin = pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            int count = dataLayerPlugin.ClearAllTables();
            Console.WriteLine($"Tables cleared, {count} records removed.");
        }
    }
}
