using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class GetLatestConfig: ICommand
    {
        public string Describe()
        {
            return @"Force gets the latest config from Git without restarting Wbtb.";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            ConfigurationBootstrapper configBootstrapper = di.Resolve<ConfigurationBootstrapper>();
            
            bool hasChanged = configBootstrapper.EnsureLatest(true);
            string status = hasChanged ? "Config was updated" : "Config unchanged";
            ConsoleHelper.WriteLine(status);
        }
    }
}
