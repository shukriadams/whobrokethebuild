using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class Core_ConfigUpdate : ICommand
    {
        private readonly Logger _logger;

        public Core_ConfigUpdate(Logger logger) 
        {
            _logger = logger;
        }

        public string Describe()
        {
            return @"Updates config while application is running. For dev only.";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            Configuration config = di.Resolve<Configuration>();
            ((IList)config.BuildServers[0].Jobs.First().Config)[2] = new KeyValuePair<string, object>("Interval", "2");

            _logger.Status($"Updated!");
        }
    }
}
