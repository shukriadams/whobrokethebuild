﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Adds revisions in build, assuming job supports revision-in-build lookup at build server. If not, must read revisions in build from 
    /// log, which has its own daemon.
    /// </summary>
    public class BuildRevisionAddDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly BuildEventHandlerHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;

        public static int TaskGroup = 0;

        #endregion

        #region CTORS

        public BuildRevisionAddDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            _di = new SimpleDI();
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>();
            _buildLevelPluginHelper = _di.Resolve<BuildEventHandlerHelper>();
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
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.AddBuildRevisionsFromBuildServer.ToString());
            TaskDaemonProcesses daemonProcesses = _di.Resolve<TaskDaemonProcesses>();

            try
            {
                foreach (DaemonTask task in tasks)
                {
                    try
                    {
                        Build build = dataLayer.GetBuildById(task.BuildId);
                        Job job = dataLayer.GetJobById(build.JobId);
                        BuildServer buildServer = dataLayer.GetBuildServerByKey(job.BuildServer);
                        IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                        ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

                        daemonProcesses.AddActive(this, $"Task : {task.Id}, Build {build.Id}");

                        if (!reach.Reachable)
                        {
                            _log.LogError($"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                            daemonProcesses.TaskBlocked(task, $"Buildserver {buildServer.Key} not reachable, job import deferred {reach.Error}{reach.Exception}");
                            continue;
                        }

                        // this daemon's tasks are not blocked by preceeding tasks

                        BuildRevisionsRetrieveResult result = buildServerPlugin.GetRevisionsInBuild(build);
                        foreach (string revisionCode in result.Revisions)
                        {
                            string biID = dataLayer.SaveBuildInvolement(new BuildInvolvement
                            {
                                BuildId = build.Id,
                                RevisionCode = revisionCode
                            }).Id;

                            dataLayer.SaveDaemonTask(new DaemonTask
                            {
                                TaskKey = DaemonTaskTypes.RevisionResolve.ToString(),
                                BuildId = build.Id,
                                BuildInvolvementId = biID,
                                Src = this.GetType().Name,
                                Order = 3,
                            });

                            dataLayer.SaveDaemonTask(new DaemonTask
                            {
                                TaskKey = DaemonTaskTypes.UserResolve.ToString(),
                                BuildId = build.Id,
                                BuildInvolvementId = biID,
                                Src = this.GetType().Name,
                                Order = 3,
                            });
                        }

                        task.HasPassed = result.Success;
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.Result = result.Result;
                        dataLayer.SaveDaemonTask(task);
                        daemonProcesses.TaskDone(task);
                    }
                    catch (Exception ex)
                    {
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = false;
                        task.Result = ex.ToString();
                        dataLayer.SaveDaemonTask(task);
                        daemonProcesses.TaskDone(task);
                    }
                }
            }
            finally
            {
                daemonProcesses.ClearActive(this);
            }
        }
        #endregion
    }
}
