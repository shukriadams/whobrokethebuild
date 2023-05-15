using System;
using Wbtb.Core.CLI.Commands;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            try 
            {
                // bind types - dev only! These are needed by all general plugin activity
                Core core = new Core();
                core.Start();

                SimpleDI di = new SimpleDI();
                di.Register<OrphanRecordHelper, OrphanRecordHelper>();
                // register local commands
                di.Register<GetLatestConfig, GetLatestConfig>();
                di.Register<IBuildServerPlugin_ListRemoteJobsCanonical, IBuildServerPlugin_ListRemoteJobsCanonical>();
                di.Register<IDataLayerPlugin_ListOrphanedRecords, IDataLayerPlugin_ListOrphanedRecords>();
                di.Register<IDataLayerPlugin_MergeSourceServers, IDataLayerPlugin_MergeSourceServers>();
                di.Register<IDataLayerPlugin_DeleteBuildServer, IDataLayerPlugin_DeleteBuildServer>();
                di.Register<IDataLayerPlugin_DeleteSourceServer, IDataLayerPlugin_DeleteSourceServer>();
                di.Register<IDataLayerPlugin_DeleteUser, IDataLayerPlugin_DeleteUser>();
                di.Register<IDataLayerPlugin_MergeBuildServers, IDataLayerPlugin_MergeBuildServers>();
                di.Register<IDataLayerPlugin_MergeUsers, IDataLayerPlugin_MergeUsers>();
                di.Register<Debug_BreakBuild, Debug_BreakBuild>();
                di.Register<Debug_FixBuild, Debug_FixBuild>();

                Config config = di.Resolve<Config>();

                ConfigurationBuilder configurationBuilder = di.Resolve<ConfigurationBuilder>();
                OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();
                PluginProvider pluginProvider = di.Resolve<PluginProvider>();
                CommandLineSwitches switches = new CommandLineSwitches(args);

                string command = string.Empty;
                if (switches.Contains("c"))
                    command = switches.Get("c");
                if (switches.Contains("command"))
                    command = switches.Get("command");

                if (string.IsNullOrEmpty(command)) 
                {
                    Console.WriteLine($"ERROR : key --\"command|c\" <COMMAND NAME> required");
                    Environment.Exit(1);
                }

                string command_safe = command.Replace(".", "_");
                Type commandType = TypeHelper.ResolveType($"{typeof(ICommand).Namespace}.{command_safe}");
                
                if (commandType == null || !di.IsServiceRegistered(commandType))
                {
                    Console.WriteLine($"ERROR : command \"{command}\" does not exist.");
                    Environment.Exit(1);
                }

                ICommand cmd = di.Resolve(commandType) as ICommand;
                cmd.Process(switches);
            }
            catch (ConfigurationException ex)
            {
                Console.WriteLine($"CONFIG ERROR : {ex.Message}");
            }
        }
    }
}
