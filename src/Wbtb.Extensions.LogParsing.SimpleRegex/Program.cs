using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.SimpleRegex
{
    public class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<SimpleRegex>().Process(args);
        }
    }
}
