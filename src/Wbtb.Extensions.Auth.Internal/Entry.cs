using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.Auth.Internal
{
    class Entry
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<Internal> { }.Process(args);
        }
    }
}
