using System;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.Unreal
{
    class Program
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<Unreal4>().Process(args);
        }
    }
}
