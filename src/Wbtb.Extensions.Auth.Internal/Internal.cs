using System;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Auth.Internal
{
    public class Internal : Plugin, IAuthenticationPlugin
    {
        public PluginInitResult InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }


        public ReachAttemptResult AttemptReach()
        {
            // no check required, 
            return new ReachAttemptResult{ Reachable = true};
        }

        public AuthenticationResult RequestPasswordLogin(string username, string password)
        {
            return new AuthenticationResult
            { 
                Success = false
            };
        }
    }
}
