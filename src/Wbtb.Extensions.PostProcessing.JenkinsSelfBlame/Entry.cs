using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.JenkinsSelfBlaming
{
    class Entry
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<AcmeGamesBlamer>().Process(args);
        }
    }
}
