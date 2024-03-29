﻿namespace Wbtb.Core.Common
{
    public class DaemonPluginProxy : PluginProxy, IDaemon
    {
        private readonly IPluginSender _pluginSender;

        public DaemonPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        public void Diagnose()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(this.Diagnose),
            });
        }

        public void Tick()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(this.Tick),
            });
        }
    }
}
