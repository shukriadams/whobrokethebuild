using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.PostProcessing.Test
{
    class Entry
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<Test>().Process(args);
        }
    }
}
