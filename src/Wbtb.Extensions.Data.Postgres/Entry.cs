using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.Data.Postgres
{
    public class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<Postgres>().Process(args);
        }
    }
}
