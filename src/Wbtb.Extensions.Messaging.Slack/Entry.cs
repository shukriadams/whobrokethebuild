using Wbtb.Core.Common;

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
