using System;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Auth.Internal
{
    public class Internal : Plugin, IAuthenticationPlugin
    {
        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        ReachAttemptResult IReachable.AttemptReach()
        {
            // no check required, 
            return new ReachAttemptResult{ Reachable = true};
        }

        AuthenticationResult IAuthenticationPlugin.RequestPasswordLogin(string username, string password)
        {
            return new AuthenticationResult
            { 
                Success = false
            };
        }
    }
}
