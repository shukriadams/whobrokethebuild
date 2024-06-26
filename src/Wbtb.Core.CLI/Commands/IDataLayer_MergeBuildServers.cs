﻿using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_MergeBuildServers : ICommand
    {
        public string Describe()
        {
            return @"Nigrate child records of an orphan build server to another build server. The orphan build server is deleted in the process.";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();

            ConsoleHelper.WriteLine("Executing function IDataLayerPlugin.MergeBuildServers");
            if (!switches.Contains("from"))
            {
                ConsoleHelper.WriteLine($"ERROR : key \"from\" required");
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("to"))
            {
                ConsoleHelper.WriteLine($"ERROR : key \"to\" required");
                Environment.Exit(1);
                return;
            }

            string fromBuildServerKey = switches.Get("from");
            string toBuildServerKey = switches.Get("to");

            try
            {
                orphanRecordHelper.MergeBuildServers(fromBuildServerKey, toBuildServerKey);
            }
            catch (RecordNotFoundException ex)
            {
                ConsoleHelper.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
