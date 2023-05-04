using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.Cpp
{
    class Entry
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<Cpp>().Process(args);
        }
    }
}
