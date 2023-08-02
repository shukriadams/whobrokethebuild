using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IMessaging_AlertPassing : ICommand
    {
        public string Describe()
        {
            return @"Sends a build break warning top the given message provider";
        }

        public void Process(CommandLineSwitches switches)
        {
            if (!switches.Contains("build"))
            {
                Console.WriteLine($"ERROR : \"build\" <buildid> required");
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("plugin"))
            {
                Console.WriteLine($"ERROR : \"plugin\" <pluginkey> required");
                Environment.Exit(1);
                return;
            }

            if (!switches.Contains("user") && !switches.Contains("group"))
            {
                Console.WriteLine($"ERROR : \"user\"  or \"group\" required");
                Environment.Exit(1);
                return;
            }

            string buildId = switches.Get("build"); ;
            string pluginKey = switches.Get("plugin"); ;
            string userKey = null;
            string groupKey = null;

            if (switches.Contains("user"))
                userKey = switches.Get("user");

            if (switches.Contains("group"))
                groupKey = switches.Get("group");

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IMessagingPlugin messagingPlugin = pluginProvider.GetByKey(pluginKey) as IMessagingPlugin;
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Build build = dataLayer.GetBuildById(buildId);

            if (messagingPlugin == null)
            {
                Console.WriteLine($"ERROR : plugin \"{pluginKey}\" not found");
                Environment.Exit(1);
                return;
            }

            if (build == null)
            {
                Console.WriteLine($"ERROR : build \"{buildId}\" not found");
                Environment.Exit(1);
                return;
            }

            // mark build as fixing itself, it's testing only
            string result = messagingPlugin.AlertPassing(userKey, groupKey, build, build); 
            

            Console.Write($"Message test executed, result : {result}");
        }
    }
}
