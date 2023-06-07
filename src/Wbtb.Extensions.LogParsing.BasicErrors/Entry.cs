using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.BasicErrors
{
    public class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<BasicErrors>().Process(args);
        }
    }
}
