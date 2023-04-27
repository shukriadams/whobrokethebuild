using System.Collections.Generic;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    public class SourceServerPluginProxy : PluginProxy, ISourceServerPlugin
    {
        public string Verify()
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            return pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "Verify"
            });
        }

        public void VerifySourceServerConfig(SourceServer contextServer)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "VerifySourceServerConfig",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "contextServer", Value = contextServer }
                }
            });
        }

        public ReachAttemptResult AttemptReach(SourceServer contextServer)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            return pluginSender.InvokeMethod<ReachAttemptResult>(this, new PluginArgs
            {
                FunctionName = "GetRevisionsBetween",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "contextServer", Value = contextServer }
                }
            });
        }

        public IEnumerable<Revision> GetRevisionsBetween(SourceServer contextServer, string revisionStart, string revisionEnd)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            return pluginSender.InvokeMethod<IEnumerable<Revision>>(this, new PluginArgs
            {
                FunctionName = "GetRevisionsBetween",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "contextServer", Value = contextServer },
                    new PluginFunctionParameter { Name = "revisionStart", Value = revisionStart },
                    new PluginFunctionParameter { Name = "revisionEnd", Value = revisionEnd }
                }
            });
        }

        public Revision GetRevision(SourceServer contextServer, string revisionCode)
        {
            IPluginSender pluginSender = PluginSenderFactory.Get();
            return pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = "GetRevision",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "contextServer", Value = contextServer },
                    new PluginFunctionParameter { Name = "revisionCode", Value = revisionCode }
                }
            });
        }

    }
}
