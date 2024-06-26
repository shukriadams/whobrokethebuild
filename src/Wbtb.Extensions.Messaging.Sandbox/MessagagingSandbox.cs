﻿using Wbtb.Core.Common;

namespace Wbtb.Extensions.Messaging.Sandbox
{
    internal class MessagingSandbox : Plugin, IMessagingPlugin
    {
        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult { Success = true };
        }

        ReachAttemptResult IReachable.AttemptReach()
        {
            return new ReachAttemptResult { Reachable = true };
        }

        string IMessagingPlugin.AlertBreaking(string user, string group, Build incidentBuild, bool isMutation, bool force)
        {
            return "alerted";
        }

        string IMessagingPlugin.AlertPassing(string user, string group, Build incidentBuild, Build fixingBuild)
        {
            return "alerted";
        }

        string IMessagingPlugin.RemindBreaking(string user, string group, Wbtb.Core.Common.Build incidentBuild, bool force)
        {
            return "alerted";
        }

        string IMessagingPlugin.DeleteAlert(object alertId)
        {
            return "deleted";
        }

        string IMessagingPlugin.TestHandler(MessageConfiguration alertHandler)
        {
            return "working";
        }

        void IMessagingPlugin.ValidateAlertConfig(MessageConfiguration alertConfig)
        {
            
        }
    }
}
