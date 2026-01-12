namespace Wbtb.Core.Common 
{ 
    public class MessagingPluginProxy : PluginProxy, IMessagingPlugin 
    {
        private readonly IPluginSender _pluginSender;

        public MessagingPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        void IPlugin.Diagnose()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(IPlugin.Diagnose)
            });
        }

        string IMessagingPlugin.TestHandler(MessageConfiguration alertHandler)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = nameof(IMessagingPlugin.TestHandler),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "alertHandler", Value = alertHandler }
                }
            });
        }

        string IMessagingPlugin.AlertBreaking(string user, string group, Build incidentBuild, bool isMutation, bool force)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = nameof(IMessagingPlugin.AlertBreaking),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "user", Value = user },
                    new PluginFunctionParameter { Name = "group", Value = group },
                    new PluginFunctionParameter { Name = "incidentBuild", Value = incidentBuild },
                    new PluginFunctionParameter { Name = "isMutation", Value = isMutation },
                    new PluginFunctionParameter { Name = "force", Value = force },
                }
            });
        }

        string IMessagingPlugin.RemindBreaking(string user, string group, Build incidentBuild, bool force)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = nameof(IMessagingPlugin.RemindBreaking),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "user", Value = user },
                    new PluginFunctionParameter { Name = "group", Value = group },
                    new PluginFunctionParameter { Name = "incidentBuild", Value = incidentBuild },
                    new PluginFunctionParameter { Name = "force", Value = force },
                }
            });
        }

        string IMessagingPlugin.AlertPassing(string user, string group, Build incidentBuild, Build fixingBuild)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = nameof(IMessagingPlugin.AlertPassing),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "user", Value = user },
                    new PluginFunctionParameter { Name = "group", Value = group },
                    new PluginFunctionParameter { Name = "incidentBuild", Value = incidentBuild },
                    new PluginFunctionParameter { Name = "fixingBuild", Value = fixingBuild }
                }
            });
        }

        string IMessagingPlugin.DeleteAlert(object alertId)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = nameof(IMessagingPlugin.DeleteAlert),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "alertId", Value = alertId }
                }
            });
        }

        void IMessagingPlugin.ValidateAlertConfig(MessageConfiguration config)
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(IMessagingPlugin.ValidateAlertConfig),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "config", Value = config }
                }
            });
        }

    }
}
