using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.Auth.ActiveDirectorySandbox
{
    class Entry
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<ActiveDirectorySandbox> { }.Process(args);
        }
    }
}
