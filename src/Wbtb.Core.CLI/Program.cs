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
                Logger logger = new Logger { 
                    AppendDates = false,
                    AppendCategory = false,
                    SendToFile = false,
                };
                
                di.RegisterSingleton<Logger>(logger);
                di.RegisterFactory<ILogger, LogProvider>();
                di.Register<OrphanRecordHelper, OrphanRecordHelper>();
                di.Register<MutationHelper, MutationHelper>();
                di.Register<ConsoleCLIHelper, ConsoleCLIHelper>();
                di.Register<Core, Core>();


                // bind types - dev only! These are needed by all general plugin activity
                Core core = di.Resolve<Core>();
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
                    logger.Status($"ERROR : key --\"command|c\" <COMMAND NAME> required");
                    logger.Status("Available commands:");
                    foreach (Type availableCommand in availableCommands.OrderBy(c => c.Name)) 
                    {
                        ICommand commandInstance = di.Resolve(availableCommand) as ICommand;
                        logger.Status($"Command : {availableCommand.Name} ({commandInstance.Describe()})");
                    }

                    Environment.Exit(1);
                    return;
                }

                string command_safe = command.Replace(".", "_");
                Type commandType = TypeHelper.ResolveType($"{typeof(ICommand).Namespace}.{command_safe}");
                
                if (commandType == null)
                {
                    logger.Status($"ERROR : command \"{command}\" does not exist.");
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
