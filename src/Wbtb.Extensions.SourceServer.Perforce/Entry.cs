using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.SourceServer.Perforce
{
    class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<Perforce>().Process(args);
        }
    }
}
