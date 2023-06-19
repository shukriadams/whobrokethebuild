using System;
using Wbtb.Core.CLI.Lib;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_ResetJob : ICommand
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly ConsoleHelper _consoleHelper;

        #endregion

        #region CTORS

        public IDataLayer_ResetJob(PluginProvider pluginProvider, ConsoleHelper consoleHelper) 
        {
            _pluginProvider = pluginProvider;
            _consoleHelper = consoleHelper;
        }

        #endregion

        #region METHODS

        public string Describe()
        {
            return @"Resets all error flags on a job, allowing linking, resolving, build log processing to run on all builds in that job. Alerts are not resent.";
        }

        public void Process(CommandLineSwitches switches) 
        {
            if (!switches.Contains("job"))
            {
                Console.WriteLine($"ERROR : \"--job\" <jobkey> required");
                _consoleHelper.PrintJobs();
                Environment.Exit(1);
                return;
            }

            string jobkey = switches.Get("job");
            bool hard = switches.Contains("hard");

            SimpleDI di = new SimpleDI();
            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            Job job = dataLayer.GetJobByKey(jobkey);
            if (job == null)
            {
                Console.WriteLine($"ERROR : \"--job\" key {jobkey} does not point to a valid job");
                _consoleHelper.PrintJobs();
                Environment.Exit(1);    
                return;
            }

            if (hard)
                Console.WriteLine("Performing hard reset");
            else
                Console.WriteLine("Performing reset - to force delete all child records under job use --hard switch");

            int deleted = dataLayer.ResetJob(job.Id, hard);

            Console.Write($"Job reset. {deleted} records deleted.");
        }

        #endregion
    }
}
