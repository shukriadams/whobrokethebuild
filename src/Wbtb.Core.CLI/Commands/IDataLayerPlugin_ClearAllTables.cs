using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            dataLayerPlugin.ClearAllTables();
            Console.WriteLine("Tables cleared");
        }
    }
}
