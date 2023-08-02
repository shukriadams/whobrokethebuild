namespace Wbtb.Core.Common
{

    [PluginProxy(typeof(MessagingPluginProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface IMessagingPlugin : IReachable, IPlugin
    {
        string TestHandler(MessageConfiguration alertHandler);

        string DeleteAlert(object alertId);

        /// <summary>
        /// Alerts either a user or group about a build breaking.
        /// </summary>
        /// <param name="user">User key in config</param>
        /// <param name="group">Group key in config</param>
        /// <param name="incidentBuild"></param>
        /// <param name="force">If true, alert will be sent even if it has already been handled.</param>
        /// <returns></returns>
        string AlertBreaking(string user, string group, Build incidentBuild, bool force);

        /// <summary>
        /// Alerts either a user or group about a build parsing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group"></param>
        /// <param name="incidentBuild"></param>
        /// <param name="fixingBuild"></param>
        /// <param name="force">If true, alert will be sent even if it has already been handled.</param>
        /// <returns></returns>
        string AlertPassing(string user, string group, Build incidentBuild, Build fixingBuild, bool force);

        void ValidateAlertConfig(MessageConfiguration alertConfig);
    }
}
