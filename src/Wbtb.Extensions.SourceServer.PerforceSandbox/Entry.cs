using Wbtb.Core.Common;

namespace Wbtb.Extensions.SourceServer.PerforceSandbox
{
    class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<PerforceSandbox>().Process(args);
        }
    }
}
