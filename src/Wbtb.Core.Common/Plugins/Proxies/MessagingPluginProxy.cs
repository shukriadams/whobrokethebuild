namespace Wbtb.Core.Common 
{ 
    public class MessagingPluginProxy : PluginProxy, IMessaging 
    {
        private readonly IPluginSender _pluginSender;

        public MessagingPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        public void Diagnose()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "Diagnose"
            });
        }

        public string TestHandler(MessageConfiguration alertHandler)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "TestHandler",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "alertHandler", Value = alertHandler }
                }
            });
        }

        public string AlertBreaking(MessageHandler alertHandler, Build incidentBuild)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "AlertBreaking",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "alertHandler", Value = alertHandler },
                    new PluginFunctionParameter { Name = "incidentBuild", Value = incidentBuild }
                }
            });
        }

        public string AlertPassing(MessageHandler alertHandler, Build incidentBuild, Build fixingBuild)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "AlertCustomPassing",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "alertHandler", Value = alertHandler },
                    new PluginFunctionParameter { Name = "incidentBuild", Value = incidentBuild },
                    new PluginFunctionParameter { Name = "fixingBuild", Value = fixingBuild }
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

        public void ValidateAlertConfig(MessageConfiguration config)
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
