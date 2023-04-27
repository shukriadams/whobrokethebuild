using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.Messaging.Slack
{
    public class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<Slack>().Process(args);
        }
    }
}
