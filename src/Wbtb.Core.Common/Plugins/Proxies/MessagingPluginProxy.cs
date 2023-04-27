namespace Wbtb.Core.Common.Plugins.Proxies
{
    public class MessagingPluginProxy : PluginProxy, IMessaging 
    {
        public string TestHandler(AlertHandler alertHandler)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            return pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "TestHandler",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "alertHandler", Value = alertHandler }
                }
            });
        }

        public string AlertBreaking(AlertHandler alertHandler, Build build)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            return pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "AlertBreaking",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "alertHandler", Value = alertHandler },
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        public string AlertPassing(AlertHandler alertHandler, Build build)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            return pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "AlertCustomPassing",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "alertHandler", Value = alertHandler },
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        public string DeleteAlert(object alertId)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            return pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "DeleteAlert",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "alertId", Value = alertId }
                }
            });
        }

        public void ValidateAlertConfig(AlertConfig config)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "ValidateAlertConfig",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "config", Value = config }
                }
            });
        }

    }
}
