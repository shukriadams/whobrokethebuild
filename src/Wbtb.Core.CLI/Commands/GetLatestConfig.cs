using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class GetLatestConfig: ICommand
    {
        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            ConfigBootstrapper configBootstrapper = di.Resolve<ConfigBootstrapper>();
            CustomEnvironmentArgs customEnvironmentArgs = di.Resolve<CustomEnvironmentArgs>();
            customEnvironmentArgs.Apply();

            bool hasChanged = configBootstrapper.EnsureLatest();
            string status = hasChanged ? "Config was updated" : "Config unchanged";
            Console.WriteLine(status);
        }
    }
}
