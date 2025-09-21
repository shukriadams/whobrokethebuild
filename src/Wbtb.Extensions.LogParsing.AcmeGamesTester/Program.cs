using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.AcmeGamesTester
{
    public class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<AcmeGamesTester>().Process(args);
        }
    }
}
