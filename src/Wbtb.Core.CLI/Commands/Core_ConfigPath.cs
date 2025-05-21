using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class Core_ConfigPath : ICommand
    {
        public string Describe()
        {
            return @"Prints the path to the current config file.";
        }

        public void Process(CommandLineSwitches switches)
        {
            ConfigurationBasic configurationBasic = new ConfigurationBasic(); 
            ConsoleHelper.WriteLine($"Config is being read from {configurationBasic.ConfigPath}", addDate: false);
        }
    }
}
