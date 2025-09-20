using Microsoft.Extensions.Logging;
using System;
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
                CommandLineSwitches switches = new CommandLineSwitches(args);

                // load custom env args as early as possible
                CustomEnvironmentArgs customEnvironmentArgs = new CustomEnvironmentArgs();
                customEnvironmentArgs.Apply(false);

                bool validate = switches.Contains("validate") && switches.Get("validate") == "true";
                SimpleDI di = new SimpleDI();
                
                di.RegisterFactory<ILogger, LogProvider>();
                di.Register<Logger, Logger>();
                di.Register<OrphanRecordHelper, OrphanRecordHelper>();
                di.Register<ConsoleHelper, ConsoleHelper>();
                di.Register<MutationHelper, MutationHelper>();
                di.Register<ConsoleCLIHelper, ConsoleCLIHelper>();
                di.Register<ConsoleCLIHelper, ConsoleCLIHelper>();


                // bind types - dev only! These are needed by all general plugin activity
                Core core = new Core();
                bool verbose = switches.Contains("verbose");
                core.Start(
                    persistStateToDatabase: false, 
                    validate : validate, 
                    verbose : false);

                // register local commands dynamically
                IEnumerable<Type> availableCommands = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface);

                foreach(Type availableCommand in availableCommands)
                    di.Register(availableCommand, availableCommand);

                string command = string.Empty;
                if (switches.Contains("c"))
                    command = switches.Get("c");
                if (switches.Contains("command"))
                    command = switches.Get("command");

                if (string.IsNullOrEmpty(command)) 
                {
                    ConsoleHelper.WriteLine($"ERROR : key --\"command|c\" <COMMAND NAME> required", addDate : false);
                    ConsoleHelper.WriteLine("Available commands :", addDate: false);
                    foreach (Type availableCommand in availableCommands.OrderBy(c => c.Name)) 
                    {
                        ICommand commandInstance = di.Resolve(availableCommand) as ICommand;
                        ConsoleHelper.WriteLine($"Command : {availableCommand.Name} ({commandInstance.Describe()})", addDate: false);
                    }

                    Environment.Exit(1);
                    return;
                }

                string command_safe = command.Replace(".", "_");
                Type commandType = TypeHelper.ResolveType($"{typeof(ICommand).Namespace}.{command_safe}");
                
                if (commandType == null)
                {
                    ConsoleHelper.WriteLine($"ERROR : command \"{command}\" does not exist.", addDate: false);
                    Environment.Exit(1);
                }

                ICommand cmd = di.Resolve(commandType) as ICommand;
                cmd.Process(switches);
            }
            catch (ConfigurationException ex)
            {
                ConsoleHelper.WriteLine($"CONFIG ERROR : {ex.Message}", addDate: false);
            }
        }
    }
}
