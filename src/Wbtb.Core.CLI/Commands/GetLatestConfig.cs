﻿using System;
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
            CustomEnvironmentArgs customEnvironmentArgs = di.Resolve<CustomEnvironmentArgs>();
            customEnvironmentArgs.Apply();

            bool hasChanged = configBootstrapper.EnsureLatest();
            string status = hasChanged ? "Config was updated" : "Config unchanged";
            Console.WriteLine(status);
        }
    }
}
