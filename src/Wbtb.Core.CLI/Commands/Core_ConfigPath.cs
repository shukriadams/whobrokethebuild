using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class Core_ConfigPath : ICommand
    {
        private readonly Logger _logger;

        public Core_ConfigPath(Logger logger) 
        {
            _logger = logger;
        }

        public string Describe()
        {
            return @"Prints the path to the current config file.";
        }

        public void Process(CommandLineSwitches switches)
        {
            ConfigurationBasic configurationBasic = new ConfigurationBasic();
            _logger.Status($"Config is being read from {configurationBasic.ConfigPath}");
        }
    }
}
