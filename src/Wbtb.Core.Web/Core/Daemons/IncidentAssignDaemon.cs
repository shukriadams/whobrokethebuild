using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Sets incidentbuild on build records, this ties builds together when an incident occurs
    /// </summary>
    public class IncidentAssignDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger<IncidentAssignDaemon> _log;

        private IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Config _config;

        #endregion

        #region CTORS

        public IncidentAssignDaemon(ILogger<IncidentAssignDaemon> log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            SimpleDI di = new SimpleDI();
            _config = di.Resolve<Config>();
            _pluginProvider = di.Resolve<PluginProvider>(); 

        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _processRunner.Start(new DaemonWork(this.Work), tickInterval);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _processRunner.Dispose();
        }

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        private void Work()
        {
            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            foreach (BuildServer cfgbuildServer in _config.BuildServers)
            {
                BuildServer buildServer = dataLayer.GetBuildServerByKey(cfgbuildServer.Key);
                // note : buildserver can be null if trying to run daemon before auto data injection has had time to run
                if (buildServer == null)
                    continue;

                IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

                int count = 100;
                if (buildServer.ImportCount.HasValue)
                    count = buildServer.ImportCount.Value;

                if (!reach.Reachable)
                {
                    _log.LogError($"Buildserver {buildServer.Key} not reachable, job import aborted {reach.Error}{reach.Exception}");
                    return;
                }

                foreach (Job job in buildServer.Jobs)
                {
                    try
                    {
                        if (!job.Enable)
                            continue;

                        Job thisjob = dataLayer.GetJobByKey(job.Key);
                        IEnumerable<Build> buildsWithoutIncident = dataLayer.GetFailingBuildsWithoutIncident(thisjob);
                        foreach(Build buildWithoutIncident in buildsWithoutIncident)
                        {
                            Build previousBuild = dataLayer.GetPreviousBuild(buildWithoutIncident);
                            string description = string.Empty;
                            if (previousBuild == null || previousBuild.Status == BuildStatus.Passed)
                            {
                                // this build is either the very first build in job and has failed (way to start!) or is the first build of sequence to fail,
                                // mark it as the incident build
                                buildWithoutIncident.IncidentBuildId = buildWithoutIncident.Id;
                                dataLayer.SaveBuild(buildWithoutIncident);
                                description = $"Build {buildWithoutIncident.Identifier} has been assigned as its own incidentbuild";
                                Console.WriteLine(description);
                                continue;
                            }
                            
                            // build is part of a continuing incident
                            if (previousBuild != null && !string.IsNullOrEmpty(previousBuild.IncidentBuildId)){
                                buildWithoutIncident.IncidentBuildId = previousBuild.IncidentBuildId;
                                dataLayer.SaveBuild(buildWithoutIncident);
                                description = $"Build {buildWithoutIncident.Identifier} has been assigned to incident {previousBuild.IncidentBuildId}";
                                Console.WriteLine(description);
                                continue;
                            }

                            // if reach here, incidentbuild could not be set, create a buildflag record that prevents build from being re-processed
                            dataLayer.SaveBuildFlag(new BuildFlag{ 
                                BuildId = buildWithoutIncident.Id,
                                Description = description,
                                Flag = BuildFlags.IncidentBuildLinkError
                            });
                            Console.WriteLine($"Build {buildWithoutIncident.Identifier} in job {job.Key} failed incident set");
                        }                        
                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"Unexpected error trying to resolve revisions for {job.Key} from buildserver {buildServer.Key}: {ex}");
                    }
                }
            }
        }

        #endregion
    }
}
