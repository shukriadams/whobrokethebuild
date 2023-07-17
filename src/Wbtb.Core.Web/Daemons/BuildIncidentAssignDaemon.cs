﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Sets incidentbuild on build records, this ties builds together when an incident occurs
    /// </summary>
    public class BuildIncidentAssignDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private SimpleDI _di;

        #endregion

        #region CTORS

        public BuildIncidentAssignDaemon(ILogger log, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;

            _di = new SimpleDI();
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>(); 

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
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask((int)DaemonTaskTypes.IncidentAssign);
            TaskDaemonProcesses daemonProcesses = _di.Resolve<TaskDaemonProcesses>();

            try
            {
                foreach (DaemonTask task in tasks)
                {
                    try 
                    {
                        Build build = dataLayer.GetBuildById(task.BuildId);

                        IEnumerable<DaemonTask> blocking = dataLayer.DaemonTasksBlocked(build.Id, (int)DaemonTaskTypes.IncidentAssign);
                        if (blocking.Any()) 
                        {
                            daemonProcesses.TaskBlocked(task, this, blocking);
                            continue;
                        }

                        daemonProcesses.AddActive(this, $"Task : {task.Id}, Build {build.Id}");
                        Build previousBuild = dataLayer.GetPreviousBuild(build);
                        if (previousBuild == null || previousBuild.Status == BuildStatus.Passed)
                        {
                            // this build is either the very first build in job and has failed (way to start!) or is the first build of sequence to fail,
                            // mark it as the incident build
                            build.IncidentBuildId = build.Id;
                            dataLayer.SaveBuild(build);

                            task.HasPassed = true;
                            task.ProcessedUtc = DateTime.UtcNow;
                            dataLayer.SaveDaemonTask(task);
                            daemonProcesses.TaskDone(task);
                            continue;
                        }

                        // previous build is a fail, but it's incident hasn't been assigned, wait
                        // todo : add some kind of max-tries here, it needs to timeout 
                        if (previousBuild != null && previousBuild.Status == BuildStatus.Failed && string.IsNullOrEmpty(previousBuild.IncidentBuildId))
                        {
                            Console.WriteLine($"Skipping task {task.Id} for build {build.Id}, previous build {previousBuild.Id} is marked as fail but doesn't yet have an incident assigned");
                            daemonProcesses.TaskBlocked(task, "Previous build waiting for incident assignment");
                            continue;
                        }

                        // set incident to whatever previous build incident is, check above ensures that if prev failed, it has an incdidentid 
                        if (previousBuild != null)
                        {
                            build.IncidentBuildId = previousBuild.IncidentBuildId;
                            dataLayer.SaveBuild(build);

                            task.HasPassed = true;
                            task.ProcessedUtc = DateTime.UtcNow;
                            dataLayer.SaveDaemonTask(task);
                            daemonProcesses.TaskDone(task);
                            continue;
                        }

                        // if reach here, incidentbuild could not be set, create a buildflag record that prevents build from being re-processed
                        task.HasPassed = false;
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.Result = "Failed to assign incident.";
                        if (previousBuild == null)
                            task.Result += "Previous build null";
                        if (previousBuild != null && string.IsNullOrEmpty(previousBuild.IncidentBuildId))
                            task.Result += "Previous build null";

                        dataLayer.SaveDaemonTask(task);
                        daemonProcesses.TaskDone(task);
                    }
                    catch (Exception ex) 
                    {
                        task.HasPassed = false;
                        task.ProcessedUtc = DateTime.UtcNow;
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