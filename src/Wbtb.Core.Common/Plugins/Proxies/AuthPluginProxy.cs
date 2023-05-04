using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    public class AuthPluginProxy : PluginProxy, IAuthenticationPlugin, IPluginProxy
    {
        private readonly IPluginSender _pluginSender;

        public AuthPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        public AuthenticationResult RequestPasswordLogin(string username, string password)
        {
            return _pluginSender.InvokeMethod<AuthenticationResult>(this, new PluginArgs
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
