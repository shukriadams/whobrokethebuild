using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.SimpleHooks
{
    public class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<SimpleHooks>().Process(args);
        }
    }
}
