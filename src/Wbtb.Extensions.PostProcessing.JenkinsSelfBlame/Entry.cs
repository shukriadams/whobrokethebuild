using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.JenkinsSelfBlame
{
    class Entry
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<JenkinsSelfBlame>().Process(args);
        }
    }
}
