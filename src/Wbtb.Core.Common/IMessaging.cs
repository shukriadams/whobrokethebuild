namespace Wbtb.Core.Common
{

    [PluginProxy(typeof(MessagingPluginProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface IMessaging : IReachable, IPlugin
    {
        string TestHandler(AlertHandler alertHandler);

        string DeleteAlert(object alertId);

        string AlertBreaking(AlertHandler alertHandler, Build incidentBuild);

        string AlertPassing(AlertHandler alertHandler, Build incidentBuild, Build fixingBuild);

        void ValidateAlertConfig(AlertConfig alertConfig);
    }
}
