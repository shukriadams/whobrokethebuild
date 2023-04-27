using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    public class DaemonPluginProxy : PluginProxy, IDaemon
    {
        public void Tick()
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "Tick"
            });
        }
    }
}
