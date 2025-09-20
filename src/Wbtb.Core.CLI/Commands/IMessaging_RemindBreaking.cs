using System;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IMessaging_RemindBreaking : ICommand
    {
        private readonly Logger _logger;

        public IMessaging_RemindBreaking(Logger logger) 
        {
            _logger = logger;
        }

        public string Describe()
        {
            return @"Sends a reminder that build is still broken, to the given message provider";
        }

        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("build"))
            {
                _logger.Status($"ERROR : \"build\" <buildid> required");
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("plugin"))
            {
                _logger.Status($"ERROR : \"plugin\" <pluginkey> required");
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("user") && !switches.Contains("group"))
            {
                _logger.Status($"ERROR : \"user\"  or \"group\" required");
                Environment.Exit(1);
                return;
            }

            string buildId = switches.Get("build");
            string pluginKey = switches.Get("plugin");
            string userKey = null;
            string groupKey = null;

            SimpleDI di = new SimpleDI();
            Configuration config = di.Resolve<Configuration>();

            if (switches.Contains("user"))
            {
                userKey = switches.Get("user");
                if (!config.Users.Any(u => u.Key == userKey))
                {
                    _logger.Status($"ERROR : user \"{userKey}\" not found");
                    Environment.Exit(1);
                    return;
                }
            }

            if (switches.Contains("group"))
            {
                groupKey = switches.Get("group");
                if (!config.Groups.Any(g => g.Key == groupKey))
                {
                    _logger.Status($"ERROR : group \"{groupKey}\" not found");
                    Environment.Exit(1);
                    return;
                }
            }

            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IMessagingPlugin messagingPlugin = pluginProvider.GetByKey(pluginKey) as IMessagingPlugin;
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Build build = dataLayer.GetBuildByUniquePublicIdentifier(buildId);

            if (messagingPlugin == null)
            {
                _logger.Status($"ERROR : plugin \"{pluginKey}\" not found");
                Environment.Exit(1);
                return;
            }

            if (build == null)
            {
                _logger.Status($"ERROR : build \"{buildId}\" not found");
                Environment.Exit(1);
                return;
            }

            if (string.IsNullOrEmpty(build.IncidentBuildId))
            {
                _logger.Status($"ERROR : cannot mark build \"{build.Id}\" as failed, build does not have an incident nr. Did this build really fail?");
                Environment.Exit(1);
                return;
            }

            string result = messagingPlugin.RemindBreaking(userKey, groupKey, build, true);

            _logger.Status($"Message test executed, result : {result}");
        }
    }
}
