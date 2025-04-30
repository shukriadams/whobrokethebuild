using System;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IMessaging_AlertPassing : ICommand
    {
        public string Describe()
        {
            return @"Sends a build pass notice to the given message provider";
        }

        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("build"))
            {
                ConsoleHelper.WriteLine($"ERROR : \"build\" <buildid> required", addDate: false);
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("plugin"))
            {
                ConsoleHelper.WriteLine($"ERROR : \"plugin\" <pluginkey> required", addDate: false);
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("user") && !switches.Contains("group"))
            {
                ConsoleHelper.WriteLine($"ERROR : \"user\"  or \"group\" required", addDate: false);
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
                ConsoleHelper.WriteLine("--fixing not set, using --build as fixing build.", addDate: false);
                fixingBuildId = incidentId;
            }

            if (switches.Contains("user"))
            {
                userKey = switches.Get("user");
                if (!config.Users.Any(u => u.Key == userKey)) 
                {
                    ConsoleHelper.WriteLine($"ERROR : user \"{userKey}\" not found", addDate: false);
                    Environment.Exit(1);
                    return;
                }

            }

            if (switches.Contains("group"))
            {
                groupKey = switches.Get("group");
                if (!config.Groups.Any(g => g.Key == groupKey))
                {
                    ConsoleHelper.WriteLine($"ERROR : group \"{groupKey}\" not found", addDate: false);
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
                ConsoleHelper.WriteLine($"ERROR : plugin \"{pluginKey}\" not found", addDate: false);
                Environment.Exit(1);
                return;
            }

            if (build == null)
            {
                ConsoleHelper.WriteLine($"ERROR : build \"{incidentId}\" not found", addDate: false);
                Environment.Exit(1);
                return;
            }

            if (fixingBuild == null && fixingBuildId != incidentId)
            {
                ConsoleHelper.WriteLine($"ERROR : fixing build \"{fixingBuild}\" not found", addDate: false);
                Environment.Exit(1);
                return;
            }

            if (string.IsNullOrEmpty(build.IncidentBuildId))
            {
                ConsoleHelper.WriteLine($"ERROR : cannot mark build \"{build.Id}\" as fixed, build does not have an incident nr. Did this build really fail?", addDate: false);
                Environment.Exit(1);
                return;
            }

            // mark build as fixing itself, it's testing only
            string result = messagingPlugin.AlertPassing(userKey, groupKey, build, fixingBuild);

            ConsoleHelper.WriteLine($"Message test executed, result : {result}", addDate: false);
        }
    }
}
