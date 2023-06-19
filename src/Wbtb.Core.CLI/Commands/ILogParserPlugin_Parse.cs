using System;
using System.IO;
using Wbtb.Core.CLI.Lib;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class ILogParserPlugin_Parse : ICommand
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly ConsoleHelper _consoleHelper;

        #endregion

        #region CTORS

        public ILogParserPlugin_Parse(PluginProvider pluginProvider, ConsoleHelper consoleHelper)
        {
            _pluginProvider = pluginProvider;
            _consoleHelper = consoleHelper;
        }

        #endregion

        #region METHODS

        public string Describe()
        {
            return @"Non-destructively parses a log with a specified plugin.";
        }

        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("parser"))
            {
                Console.WriteLine($"ERROR : \"--parser\" <parsername> required");
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("build"))
            {
                Console.WriteLine($"ERROR : \"--build\" <buildid> required");
                Environment.Exit(1);
                return;
            }

            string parserName = switches.Get("parser");
            string buildId = switches.Get("build");
            IDataPlugin data = _pluginProvider.GetDistinct<IDataPlugin>();
            Build build = data.GetBuildById(buildId);
            if (build == null)
            {
                Console.WriteLine($"No build with id {buildId} found. Note this is a unique database id, not key/identifier from an external build srver.");
                Environment.Exit(1);
                return;    
            }

            ILogParserPlugin parser = _pluginProvider.GetByKey(parserName) as ILogParserPlugin;
            if (parser == null) 
            {
                Console.WriteLine($"No parser with name {parserName} found.");
                Environment.Exit(1);
                return;
            }

            if (string.IsNullOrEmpty(build.LogPath)) 
            {
                Console.WriteLine($"Build {buildId} does not have a log.");
                Environment.Exit(1);
                return;
            }

            string log = File.ReadAllText(build.LogPath);
            string result = parser.Parse(log);
            Console.WriteLine("Parsed log, got :");
            Console.Write(result);
        }

        #endregion
    }
}
