using System.Collections.Generic;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    public class BuildServerPluginProxy : PluginProxy , IBuildServerPlugin
    {
        private readonly IPluginSender _pluginSender;

        public BuildServerPluginProxy(IPluginSender pluginSender) : base(pluginSender)
        {
            _pluginSender = pluginSender;
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

        IEnumerable<User> IBuildServerPlugin.GetUsersInBuild(BuildServer buildServer, string remoteBuildId)
        {
            return _pluginSender.InvokeMethod<IEnumerable<User>>(this, new PluginArgs
            {
                FunctionName = "GetUsersInBuild",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "buildServer", Value = buildServer },
                    new PluginFunctionParameter { Name = "remoteBuildId", Value = remoteBuildId }
                }
            });
        }

        BuildImportSummary IBuildServerPlugin.ImportBuilds(Job job, int take)
        {
            return _pluginSender.InvokeMethod<BuildImportSummary>(this, new PluginArgs
            {
                FunctionName = "ImportBuilds",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job },
                    new PluginFunctionParameter { Name = "take", Value = take }
                }
            });
        }

        BuildImportSummary IBuildServerPlugin.ImportAllCachedBuilds(Job job)
        {
            return _pluginSender.InvokeMethod<BuildImportSummary>(this, new PluginArgs
            {
                FunctionName = "ImportAllCachedBuilds",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
                }
            });
        }
        
        IEnumerable<Build> IBuildServerPlugin.ImportLogs(Job job)
        {
            return _pluginSender.InvokeMethod<IEnumerable<Build>>(this, new PluginArgs
            {
                FunctionName = "ImportLogs",
                Arguments = new PluginFunctionParameter[] {
                    new PluginFunctionParameter { Name = "job", Value = job }
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
    }
}
