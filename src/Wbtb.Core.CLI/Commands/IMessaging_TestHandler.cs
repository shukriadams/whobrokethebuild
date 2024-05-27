using System;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IMessaging_TestHandler : ICommand
    {
        public string Describe()
        {
            return @"Sends a test message to a message handler. Use this to test integrations like email, slack etc.";
        }
        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("user") && !switches.Contains("group"))
            {
                ConsoleHelper.WriteLine($"ERROR : \"user\"  or \"group\" required");
                Environment.Exit(1);
                return;
            }

            string userKey = null;
            string groupKey = null;
            if (switches.Contains("user"))
                userKey = switches.Get("user");

            if (switches.Contains("group"))
                groupKey = switches.Get("group");

            if (!switches.Contains("plugin"))
            {
                ConsoleHelper.WriteLine("ERROR : \"plugin\" required");
                Environment.Exit(1);
                return;
            }
            string pluginKey = switches.Get("plugin");

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IMessagingPlugin messagingPlugin = pluginProvider.GetByKey(pluginKey) as IMessagingPlugin;
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Configuration configuration = di.Resolve<Configuration>();

            if (messagingPlugin == null) 
            {
                ConsoleHelper.WriteLine($"ERROR : plugin \"{pluginKey}\" not found");
                Environment.Exit(1);
                return;
            }

            MessageConfiguration messageConfiguration = null;
            User user = null;
            Group group = null;
            if (!string.IsNullOrEmpty(userKey))
            {
                user = dataLayer.GetUserByKey(userKey);
                if (user == null)
                {
                    ConsoleHelper.WriteLine($"ERROR : user \"{userKey}\" not found");
                    Environment.Exit(1);
                    return;
                }

                messageConfiguration = user.Message.Where(m => m.Plugin == pluginKey).FirstOrDefault();
            }
            else 
            {
                group = configuration.Groups.Where(g => g.Key == groupKey).FirstOrDefault();
                if (group == null)
                {
                    ConsoleHelper.WriteLine($"ERROR : group \"{groupKey}\" not found");
                    Environment.Exit(1);
                    return;
                }
                messageConfiguration = group.Message.Where(m => m.Plugin == pluginKey).FirstOrDefault();
            }

            if (messageConfiguration == null) 
            {
                ConsoleHelper.WriteLine($"Target recipient does not have a message configuration for plugin \"{pluginKey}\".");
                Environment.Exit(1);
                return;
            }

            string result = messagingPlugin.TestHandler(messageConfiguration);
            ConsoleHelper.WriteLine($"Message test execute, result : {result}");
        }
    }
}
