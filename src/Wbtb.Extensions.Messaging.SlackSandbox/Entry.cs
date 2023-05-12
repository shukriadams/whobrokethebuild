using Wbtb.Core.Common;

namespace Wbtb.Extensions.Messaging.SlackSandbox
{
    public class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<SlackSandbox>().Process(args);
        }
    }
}
