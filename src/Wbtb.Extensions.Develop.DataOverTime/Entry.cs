using Wbtb.Core.Common;

namespace Wbtb.Extensions.Develop.DataOverTime
{
    public class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<DataOverTime>().Process(args);
        }
    }
}
