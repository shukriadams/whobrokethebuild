using Wbtb.Core.Common;

namespace Wbtb.Extensions.Messaging.Sandbox
{
    internal class MessagingSandbox : Plugin, IMessaging
    {
        public string AlertBreaking(AlertHandler alertHandler, Build incidentBuild)
        {
            return "alerted";
        }

        public string AlertPassing(AlertHandler alertHandler, Build incidentBuild, Build fixingBuild)
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

        public string TestHandler(AlertHandler alertHandler)
        {
            return "working";
        }

        public void ValidateAlertConfig(AlertConfig alertConfig)
        {
            
        }
    }
}
