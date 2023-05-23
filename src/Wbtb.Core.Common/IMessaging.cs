namespace Wbtb.Core.Common
{

    [PluginProxy(typeof(MessagingPluginProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface IMessaging : IReachable, IPlugin
    {
        string TestHandler(MessageConfiguration alertHandler);

        string DeleteAlert(object alertId);

        string AlertBreaking(MessageHandler alertHandler, Build incidentBuild);

        string AlertPassing(MessageHandler alertHandler, Build incidentBuild, Build fixingBuild);

        void ValidateAlertConfig(MessageConfiguration alertConfig);
    }
}
