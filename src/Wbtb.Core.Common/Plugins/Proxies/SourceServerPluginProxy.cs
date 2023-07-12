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

        void IPlugin.Diagnose()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(IPlugin.Diagnose)
            });
        }

        void ISourceServerPlugin.VerifySourceServerConfig(SourceServer contextServer)
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(ISourceServerPlugin.VerifySourceServerConfig),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "contextServer", Value = contextServer }
                }
            });
        }

        void ISourceServerPlugin.VerifyJobConfig(Job job)
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(ISourceServerPlugin.VerifyJobConfig),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        ReachAttemptResult ISourceServerPlugin.AttemptReach(SourceServer contextServer)
        {
            return _pluginSender.InvokeMethod<ReachAttemptResult>(this, new PluginArgs
            {
                FunctionName = nameof(ISourceServerPlugin.AttemptReach),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "contextServer", Value = contextServer }
                }
            });
        }

        IEnumerable<Revision> ISourceServerPlugin.GetRevisionsBetween(Job job, string revisionStart, string revisionEnd)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Revision>>(this, new PluginArgs
            {
                FunctionName = nameof(ISourceServerPlugin.GetRevisionsBetween),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job },
                    new PluginFunctionParameter { Name = "revisionStart", Value = revisionStart },
                    new PluginFunctionParameter { Name = "revisionEnd", Value = revisionEnd }
                }
            });
        }

        Revision ISourceServerPlugin.GetRevision(SourceServer contextServer, string revisionCode)
        {
            return _pluginSender.InvokeMethod<Revision>(this, new PluginArgs
            {
                FunctionName = nameof(ISourceServerPlugin.GetRevision),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "contextServer", Value = contextServer },
                    new PluginFunctionParameter { Name = "revisionCode", Value = revisionCode }
                }
            });
        }

    }
}
