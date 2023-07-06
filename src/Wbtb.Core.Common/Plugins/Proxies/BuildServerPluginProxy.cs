using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class BuildServerPluginProxy : PluginProxy , IBuildServerPlugin
    {
        private readonly IPluginSender _pluginSender;

        public BuildServerPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
        }

        public void Diagnose()
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(this.Diagnose)
            });
        }

        void IBuildServerPlugin.VerifyBuildServerConfig(BuildServer buildServer)
        {
            _pluginSender.InvokeMethod<ReachAttemptResult>(this, new PluginArgs
            {
                FunctionName = nameof(IBuildServerPlugin.VerifyBuildServerConfig),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildServer", Value = buildServer }
                }
            });
        }

        ReachAttemptResult IBuildServerPlugin.AttemptReach(BuildServer contextServer)
        {
            return _pluginSender.InvokeMethod<ReachAttemptResult>(this, new PluginArgs
            {
                FunctionName = nameof(IBuildServerPlugin.AttemptReach),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "contextServer", Value = contextServer }
                }
            });
        }

        void IBuildServerPlugin.AttemptReachJob(BuildServer buildServer, Job job)
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(IBuildServerPlugin.AttemptReachJob),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildServer", Value = buildServer },
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        void IBuildServerPlugin.PollBuildsForJob(Job job) 
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = nameof(IBuildServerPlugin.PollBuildsForJob),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        Build IBuildServerPlugin.TryUpdateBuild(Build build) 
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = nameof(IBuildServerPlugin.TryUpdateBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        string IBuildServerPlugin.GetBuildUrl(BuildServer contextServer, Build build)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = nameof(IBuildServerPlugin.GetBuildUrl),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "contextServer", Value = contextServer },
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        IEnumerable<string> IBuildServerPlugin.ListRemoteJobsCanonical(BuildServer buildserver)
        {
            return _pluginSender.InvokeMethod<IEnumerable<string>>(this, new PluginArgs
            {
                FunctionName = nameof(IBuildServerPlugin.ListRemoteJobsCanonical),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildserver", Value = buildserver }
                }
            });
        }

        IEnumerable<Build> IBuildServerPlugin.GetLatesBuilds(Job job, int take)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = nameof(IBuildServerPlugin.GetLatesBuilds),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job },
                    new PluginFunctionParameter { Name = "take", Value = take }
                }
            });
        }

        IEnumerable<Build> IBuildServerPlugin.GetAllCachedBuilds(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = nameof(IBuildServerPlugin.GetAllCachedBuilds),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        BuildLogRetrieveResult IBuildServerPlugin.ImportLog(Build build)
        {
            return _pluginSender.InvokeMethod<BuildLogRetrieveResult>(this, new PluginArgs
            {
                FunctionName = nameof(IBuildServerPlugin.ImportLog),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        string IBuildServerPlugin.GetEphemeralBuildLog(Build build)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = nameof(IBuildServerPlugin.GetEphemeralBuildLog),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        BuildRevisionsRetrieveResult IBuildServerPlugin.GetRevisionsInBuild(Build build)
        {
            return _pluginSender.InvokeMethod<BuildRevisionsRetrieveResult>(this, new PluginArgs
            {
                FunctionName = nameof(IBuildServerPlugin.GetRevisionsInBuild),
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

    }
}
