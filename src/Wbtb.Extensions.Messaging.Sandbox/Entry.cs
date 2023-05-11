using Wbtb.Core.Common;

namespace Wbtb.Extensions.Messaging.Sandbox
{
    public class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<MessagagingSandbox>().Process(args);
        }
    }
}
