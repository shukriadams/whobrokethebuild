namespace Wbtb.Core.Common 
{ 
    public class MessagingPluginProxy : PluginProxy, IMessaging 
    {
        private readonly IPluginSender _pluginSender;

        public MessagingPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        public string TestHandler(AlertHandler alertHandler)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "TestHandler",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "alertHandler", Value = alertHandler }
                }
            });
        }

        public string AlertBreaking(AlertHandler alertHandler, Build build)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
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
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
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
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "DeleteAlert",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "alertId", Value = alertId }
                }
            });
        }

        public void ValidateAlertConfig(AlertConfig config)
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "ValidateAlertConfig",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "config", Value = config }
                }
            });
        }

    }
}
