using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.BuildServer.Jenkins
{
    class Entry
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<Jenkins>().Process(args);
        }
    }
}
