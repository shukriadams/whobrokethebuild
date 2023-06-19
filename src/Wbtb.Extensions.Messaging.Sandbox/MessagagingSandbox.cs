using Wbtb.Core.Common;

namespace Wbtb.Extensions.Messaging.Sandbox
{
    internal class MessagingSandbox : Plugin, IMessagingPlugin
    {
        public string AlertBreaking(MessageHandler alertHandler, Build incidentBuild)
        {
            return "alerted";
        }

        public string AlertPassing(MessageHandler alertHandler, Build incidentBuild, Build fixingBuild)
        {
            return "alerted";
        }

        public ReachAttemptResult AttemptReach()
        {
            return new ReachAttemptResult { Reachable = true };
        }

        public string DeleteAlert(object alertId)
        {
            return "deleted";
        }

        public PluginInitResult InitializePlugin()
        {
            return new PluginInitResult { Success = true };
        }

        public string TestHandler(MessageConfiguration alertHandler)
        {
            return "working";
        }

        public void ValidateAlertConfig(MessageConfiguration alertConfig)
        {
            
        }
    }
}
