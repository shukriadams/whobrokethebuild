using System;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.SourceServer.PerforceSandbox
{
    class Entry
    {
        public static void Main(string[] args)
        {
            new PluginShellReceiver<PerforceSandbox>().Process(args);
        }
    }
}
