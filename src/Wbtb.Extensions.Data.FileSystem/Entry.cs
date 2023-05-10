using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.FileSystem
{
    public class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<FileSystem>().Process(args);
        }
    }
}
