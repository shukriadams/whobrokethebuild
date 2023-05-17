﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Imports full revision objects from source control system, creates them in DB, and links them to buildinvolvements
    /// where applicable.
    /// </summary>
    public class RevisionResolveDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;

        private readonly Config _config;
        
        private readonly PluginProvider _pluginProvider;

        #endregion

        #region CTORS

        public RevisionResolveDaemon(ILogger log, IDaemonProcessRunner processRunner)
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

            // start daemons - this should be folded into start
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
                        Job thisjob = dataLayer.GetJobByKey(job.Key);
                        SourceServer sourceServer = dataLayer.GetSourceServerById(thisjob.SourceServerId);
                        ISourceServerPlugin sourceServerPlugin = _pluginProvider.GetByKey(sourceServer.Plugin) as ISourceServerPlugin;

                        // Note : build involvements will normally be generated by the ImportBuilds method of the builderver plugin servicing this job
                        // Build involvements can also be generated by the BuildRevisionFromLogDaemon
                        IEnumerable<BuildInvolvement> buildInvolvements = dataLayer.GetBuildInvolvementsWithoutMappedRevisions(thisjob.Id);

                        foreach(BuildInvolvement buildInvolvement in buildInvolvements)
                        {
                            Revision revision = dataLayer.GetRevisionByKey(buildInvolvement.RevisionCode);
                            if (revision == null)
                            {
                                revision = sourceServerPlugin.GetRevision(sourceServer, buildInvolvement.RevisionCode);
                                if (revision == null)
                                {
                                    // mark revision link as failing or abandoned
                                    dataLayer.SaveBuildFlag(new BuildFlag
                                    {
                                        BuildId = buildInvolvement.BuildId,
                                        Flag = BuildFlags.RevisionNotFound,
                                        Description = $"Could not find revision \"{buildInvolvement.RevisionCode}\" in source control \"{sourceServer.Name}\" - revision linking abandoned."
                                    });

                                    continue;
                                }
                            }

                            if (revision.SourceServerId != sourceServer.Id)
                            {
                                revision.SourceServerId = sourceServer.Id;
                                dataLayer.SaveRevision(revision);
                            }

                            buildInvolvement.RevisionId = revision.Id;
                            buildInvolvement.RevisionLinkStatus = LinkState.Completed;

                            dataLayer.SaveBuildInvolement(buildInvolvement);
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