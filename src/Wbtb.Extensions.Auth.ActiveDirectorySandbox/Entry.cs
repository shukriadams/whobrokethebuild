﻿using Wbtb.Core.Common;

namespace Wbtb.Extensions.Auth.ActiveDirectorySandbox
{
    class Entry
    {
        static void Main(string[] args)
        {
            new PluginShellReceiver<ActiveDirectorySandbox> { }.Process(args);
        }
    }
}
