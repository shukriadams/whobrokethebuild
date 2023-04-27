using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(DaemonPluginProxy))]
    [PluginBehaviour(true)]
    public interface IDaemon : IPlugin 
    {
        void Tick();
    }
}
