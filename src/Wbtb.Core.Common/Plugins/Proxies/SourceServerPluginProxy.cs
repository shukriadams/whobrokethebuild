using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class SourceServerPluginProxy : PluginProxy, ISourceServerPlugin
    {
        private readonly IPluginSender _pluginSender;

        public SourceServerPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }
        
        public string Verify()
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "Verify"
            });
        }

        public void VerifySourceServerConfig(SourceServer contextServer)
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "VerifySourceServerConfig",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "contextServer", Value = contextServer }
                }
            });
        }

        public ReachAttemptResult AttemptReach(SourceServer contextServer)
        {
            return _pluginSender.InvokeMethod<ReachAttemptResult>(this, new PluginArgs
            {
                FunctionName = "GetRevisionsBetween",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "contextServer", Value = contextServer }
                }
            });
        }

        public IEnumerable<Revision> GetRevisionsBetween(SourceServer contextServer, string revisionStart, string revisionEnd)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Revision>>(this, new PluginArgs
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
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
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
