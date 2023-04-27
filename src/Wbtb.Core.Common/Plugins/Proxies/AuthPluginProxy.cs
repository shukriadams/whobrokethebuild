using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    public class AuthPluginProxy : PluginProxy, IAuthenticationPlugin, IPluginProxy
    {
        public AuthenticationResult RequestPasswordLogin(string username, string password)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();

            return pluginSender.InvokeMethod<AuthenticationResult>(this, new PluginArgs
            {
                FunctionName = "RequestPasswordLogin",
                Arguments = new PluginFunctionParameter[] { 
                    new PluginFunctionParameter { Name = "username", Value = username }, 
                    new PluginFunctionParameter { Name = "password", Value = password } 
                }
            });
        }


    }
}
