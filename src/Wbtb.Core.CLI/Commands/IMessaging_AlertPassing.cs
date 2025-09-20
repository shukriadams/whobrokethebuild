using System;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IMessaging_AlertPassing : ICommand
    {
        private readonly Logger _logger;

        public IMessaging_AlertPassing(Logger logger) 
        {
            _logger = logger;
        }

        public string Describe()
        {
            return @"Sends a build pass notice to the given message provider";
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

            string incidentId = switches.Get("build");
            string fixingBuildId = null;
            string pluginKey = switches.Get("plugin");
            string userKey = null;
            string groupKey = null;
            
            SimpleDI di = new SimpleDI();
            Configuration config = di.Resolve<Configuration>();

            if (switches.Contains("fixing"))
            {
                fixingBuildId = switches.Get("fixing");
            }
            else 
            {
                _logger.Status("--fixing not set, using --build as fixing build.");
                fixingBuildId = incidentId;
            }

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
            Build build = dataLayer.GetBuildByUniquePublicIdentifier(incidentId);
            Build fixingBuild = dataLayer.GetBuildByUniquePublicIdentifier(fixingBuildId);

            if (messagingPlugin == null)
            {
                _logger.Status($"ERROR : plugin \"{pluginKey}\" not found");
                Environment.Exit(1);
                return;
            }

            if (build == null)
            {
                _logger.Status($"ERROR : build \"{incidentId}\" not found");
                Environment.Exit(1);
                return;
            }

            if (fixingBuild == null && fixingBuildId != incidentId)
            {
                _logger.Status($"ERROR : fixing build \"{fixingBuild}\" not found");
                Environment.Exit(1);
                return;
            }

            if (string.IsNullOrEmpty(build.IncidentBuildId))
            {
                _logger.Status($"ERROR : cannot mark build \"{build.Id}\" as fixed, build does not have an incident nr. Did this build really fail?");
                Environment.Exit(1);
                return;
            }

            // mark build as fixing itself, it's testing only
            string result = messagingPlugin.AlertPassing(userKey, groupKey, build, fixingBuild);

            _logger.Status($"Message test executed, result : {result}");
        }
    }
}
