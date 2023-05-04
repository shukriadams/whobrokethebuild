using Wbtb.Core.Common;

namespace Wbtb.Extensions.BuildServer.JenkinsSandbox
{
    class Entry
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<JenkinsSandbox>().Process(args);
        }
    }
}
