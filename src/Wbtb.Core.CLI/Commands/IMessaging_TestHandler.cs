using System;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IMessaging_TestHandler : ICommand
    {
        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("user") && !switches.Contains("group"))
            {
                Console.WriteLine($"ERROR : \"user\"  or \"group\" required");
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
                Console.WriteLine("ERROR : \"plugin\" required");
                Environment.Exit(1);
                return;
            }
            string pluginKey = switches.Get("plugin");

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IMessaging messagingPlugin = pluginProvider.GetByKey(pluginKey) as IMessaging;
            IDataLayerPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            Configuration configuration = di.Resolve<Configuration>();

            if (messagingPlugin == null) 
            {
                Console.WriteLine($"ERROR : plugin \"{pluginKey}\" not found");
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
                    Console.WriteLine($"ERROR : user \"{userKey}\" not found");
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
                    Console.WriteLine($"ERROR : group \"{groupKey}\" not found");
                    Environment.Exit(1);
                    return;
                }
                messageConfiguration = group.Message.Where(m => m.Plugin == pluginKey).FirstOrDefault();
            }

            if (messageConfiguration == null) 
            {
                Console.Write($"Target recipient does not have a message configuration for plugin \"{pluginKey}\".");
                Environment.Exit(1);
                return;
            }

            MessageConfiguration messageHandler = null;
            string result = messagingPlugin.TestHandler(messageConfiguration);
            
            Console.Write($"Message test executed, result : {result}");
        }
    }
}
