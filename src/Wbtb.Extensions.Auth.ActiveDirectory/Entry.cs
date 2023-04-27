using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.Auth.ActiveDirectory
{
    class Entry
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<ActiveDirectory>{ }.Process(args);
        }
    }
}
