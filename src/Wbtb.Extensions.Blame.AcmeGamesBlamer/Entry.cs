using Wbtb.Core.Common;

namespace Wbtb.Extensions.Blame.AcmeGamesBlamer
{
    class Entry
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<AcmeGamesBlamer>().Process(args);
        }
    }
}
