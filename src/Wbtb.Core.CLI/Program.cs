﻿using System;
using System.Collections.Generic;
using System.Linq;
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
                core.Start(failOnOrphans:false);

                SimpleDI di = new SimpleDI();
                di.Register<OrphanRecordHelper, OrphanRecordHelper>();

                // register local commands dynamically
                IEnumerable<Type> availableCommands = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface);

                foreach(Type availableCommand in availableCommands)
                    di.Register(availableCommand, availableCommand);

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
                    Console.WriteLine("Available commands :");
                    foreach (Type availableCommand in availableCommands)
                        Console.WriteLine(availableCommand.Name);

                    Environment.Exit(1);
                }

                string command_safe = command.Replace(".", "_");
                Type commandType = TypeHelper.ResolveType($"{typeof(ICommand).Namespace}.{command_safe}");
                
                if (commandType == null)
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
