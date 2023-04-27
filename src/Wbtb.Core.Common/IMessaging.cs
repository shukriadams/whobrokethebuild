using Wbtb.Core.Common.Plugins;
using Wbtb.Core.Common.Plugins.Proxies;

namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(MessagingPluginProxy))]
    [PluginBehaviour(true)]

    public interface IMessaging : IReachable, IPlugin
    {
        string TestHandler(AlertHandler alertHandler);

        string DeleteAlert(object alertId);

        string AlertBreaking(AlertHandler alertHandler, Build build);

        string AlertPassing(AlertHandler alertHandler, Build build);

        void ValidateAlertConfig(AlertConfig alertConfig);
    }
}
