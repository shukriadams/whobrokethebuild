using System;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IMessaging_TestHandler : ICommand
    {
        private readonly Logger _logger;

        public IMessaging_TestHandler(Logger logger) 
        {
            _logger = logger;
        }

        public string Describe()
        {
            return @"Sends a test message to a message handler. Use this to test integrations like email, slack etc.";
        }
        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("user") && !switches.Contains("group"))
            {
                _logger.Status($"ERROR : \"user\"  or \"group\" required");
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
                _logger.Status("ERROR : \"plugin\" required");
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
                _logger.Status($"ERROR : plugin \"{pluginKey}\" not found");
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
                    _logger.Status($"ERROR : user \"{userKey}\" not found");
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
                    _logger.Status($"ERROR : group \"{groupKey}\" not found");
                    Environment.Exit(1);
                    return;
                }
                messageConfiguration = group.Message.Where(m => m.Plugin == pluginKey).FirstOrDefault();
            }

            if (messageConfiguration == null) 
            {
                _logger.Status($"Target recipient does not have a message configuration for plugin \"{pluginKey}\".");
                Environment.Exit(1);
                return;
            }

            string result = messagingPlugin.TestHandler(messageConfiguration);
            _logger.Status($"Message test execute, result : {result}");
        }
    }
}
