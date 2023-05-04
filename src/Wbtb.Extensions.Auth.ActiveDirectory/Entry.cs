using Wbtb.Core.Common;

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
