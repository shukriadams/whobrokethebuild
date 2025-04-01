using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class Core_ConfigUpdate : ICommand
    {
        public string Describe()
        {
            return @"Updates config while application is running. For dev only.";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            Configuration config = di.Resolve<Configuration>();
            ((IList)config.BuildServers[0].Jobs.First().Config)[2] = new KeyValuePair<string, object>("Interval", "2");

            ConsoleHelper.WriteLine($"Updated!");
        }
    }
}
