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
                FunctionName = "Diagnose"
            });
        }

        void IBuildServerPlugin.VerifyBuildServerConfig(BuildServer buildServer)
        {
            _pluginSender.InvokeMethod<ReachAttemptResult>(this, new PluginArgs
            {
                FunctionName = "VerifyBuildServerConfig",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildServer", Value = buildServer }
                }
            });
        }

        ReachAttemptResult IBuildServerPlugin.AttemptReach(BuildServer contextServer)
        {
            return _pluginSender.InvokeMethod<ReachAttemptResult>(this, new PluginArgs
            {
                FunctionName = "AttemptReach",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "contextServer", Value = contextServer }
                }
            });
        }

        void IBuildServerPlugin.AttemptReachJob(BuildServer buildServer, Job job)
        {
            _pluginSender.InvokeMethod(this, new PluginArgs
            {
                FunctionName = "AttemptReachJob",
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
                FunctionName = "PollBuildsForJob",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }

        Build IBuildServerPlugin.TryUpdateBuild(Build build) 
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "TryUpdateBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        string IBuildServerPlugin.GetBuildUrl(BuildServer contextServer, Build build)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "GetBuildUrl",
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
                FunctionName = "ListRemoteJobsCanonical",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildserver", Value = buildserver }
                }
            });
        }

        IEnumerable<Build> IBuildServerPlugin.GetLatesBuilds(Job job, int take)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "GetLatesBuilds",
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
                FunctionName = "GetAllCachedBuilds",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }
        
        Build IBuildServerPlugin.ImportLog(Build build)
        {
            return _pluginSender.InvokeMethod<Build>(this, new PluginArgs
            {
                FunctionName = "ImportLog",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        string IBuildServerPlugin.GetEphemeralBuildLog(Build build)
        {
            return _pluginSender.InvokeMethod<string>(this, new PluginArgs
            {
                FunctionName = "GetEphemeralBuildLog",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

        IEnumerable<string> IBuildServerPlugin.GetRevisionsInBuild(Build build)
        {
            return _pluginSender.InvokeMethod<IEnumerable<string>>(this, new PluginArgs
            {
                FunctionName = "GetRevisionsInBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "build", Value = build }
                }
            });
        }

    }
}
