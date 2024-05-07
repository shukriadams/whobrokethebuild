namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(MessagingPluginProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface IMessagingPlugin : IReachable, IPlugin
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alertHandler"></param>
        /// <returns></returns>
        string TestHandler(MessageConfiguration alertHandler);

        /// <summary>
        /// Attempts to withdraw the alert if target implementation's messaging platform supports deleting. Email f.egs does not.
        /// </summary>
        /// <param name="alertId"></param>
        /// <returns></returns>
        string DeleteAlert(object alertId);

        /// <summary>
        /// Alerts either a user or group about a build breaking.
        /// </summary>
        /// <param name="user">User key in config</param>
        /// <param name="group">Group key in config</param>
        /// <param name="incidentBuild"></param>
        /// <param name="force">If true, alert will be sent even if it has already been handled.</param>
        /// <returns></returns>
        string AlertBreaking(string user, string group, Build incidentBuild, bool isMutation, bool force);

        /// <summary>
        /// Alerts either a user or group that a build is still breaking after some interval. Internval must be defined in plugin config for job. See
        /// README for this plugin for more information.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group"></param>
        /// <param name="incidentBuild"></param>
        /// <param name="isMutation"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        string RemindBreaking(string user, string group, Build incidentBuild, bool force);

        /// <summary>
        /// Alerts either a user or group about a build parsing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group"></param>
        /// <param name="incidentBuild"></param>
        /// <param name="fixingBuild"></param>
        /// <param name="force">If true, alert will be sent even if it has already been handled.</param>
        /// <returns></returns>
        string AlertPassing(string user, string group, Build incidentBuild, Build fixingBuild);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alertConfig"></param>
        void ValidateAlertConfig(MessageConfiguration alertConfig);
    }
}
