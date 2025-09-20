using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class GetLatestConfig: ICommand
    {
        private readonly Logger _logger;

        public GetLatestConfig(Logger logger) 
        {
            _logger = logger;
        }

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
            _logger.Status(status);
        }
    }
}
