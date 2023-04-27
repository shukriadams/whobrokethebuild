using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Auth provider for users, egs, AD, Github, internal (wbtb) etc
    /// </summary>
    [PluginProxy(typeof(AuthPluginProxy))]
    [PluginBehaviour(false)]
    public interface IAuthenticationPlugin : IPlugin, IReachable
    {
        AuthenticationResult RequestPasswordLogin(string username, string password);
    }

}
