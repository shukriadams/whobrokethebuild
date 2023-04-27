namespace Wbtb.Core.Common.Plugins
{
    public class PluginSenderFactory
    {
        public static IPluginSender Get()
        {
            if (ConfigKeeper.Instance.IsCurrentContextProxyPlugin)
                return new PluginCoreSender();

            if (ConfigBasic.Instance.ProxyMode == "direct")
                return new PluginDirectSender();

            return new PluginShellSender();
        }  
    }
}
