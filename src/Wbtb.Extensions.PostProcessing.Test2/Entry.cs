using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.Test2
{
    class Entry
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<Test2>().Process(args);
        }
    }
}
