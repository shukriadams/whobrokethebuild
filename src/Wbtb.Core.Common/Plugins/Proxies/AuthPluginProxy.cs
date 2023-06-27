namespace Wbtb.Core.Common
{
    public class AuthPluginProxy : PluginProxy, IAuthenticationPlugin, IPluginProxy
    {
        #region FIELDS

        private readonly IPluginSender _pluginSender;

        #endregion

        #region CTORS

        public AuthPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        #endregion

        #region METHODS

        void IPlugin.Diagnose()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(IPlugin.Diagnose)
            });
        }

        AuthenticationResult IAuthenticationPlugin.RequestPasswordLogin(string username, string password)
        {
            return _pluginSender.InvokeMethod<AuthenticationResult>(this, new PluginArgs
            {
                FunctionName = nameof(IAuthenticationPlugin.RequestPasswordLogin),
                Arguments = new PluginFunctionParameter[] { 
                    new PluginFunctionParameter { Name = "username", Value = username }, 
                    new PluginFunctionParameter { Name = "password", Value = password } 
                }
            });
        }

        #endregion
    }
}
