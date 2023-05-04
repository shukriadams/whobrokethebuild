using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.JenkinsSelfFailing
{
    public class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<JenkinsSelfFailing>().Process(args);
        }
    }
}
