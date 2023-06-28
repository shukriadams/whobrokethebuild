using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Sets incidentbuild on build records, this ties builds together when an incident occurs
    /// </summary>
    public class IncidentAssignDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        public static int TaskGroup = 1;

        private SimpleDI _di;

        #endregion

        #region CTORS

        public IncidentAssignDaemon(ILogger log, IDaemonProcessRunner processRunner)
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
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.IncidentAssign.ToString());
            DaemonActiveProcesses activeItems = _di.Resolve<DaemonActiveProcesses>();

            try
            {
                foreach (DaemonTask task in tasks)
                {
                    try 
                    {
                        Build build = dataLayer.GetBuildById(task.BuildId);
                        activeItems.Add(this, $"Task : {task.Id}, Build {build.Id}");

                        if (dataLayer.DaemonTasksBlocked(build.Id, TaskGroup))
                            continue;

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

                            continue;
                        }

                        // previous build is a fail, but it's incident hasn't been assigned, wait
                        // todo : add some kind of max-tries here, it needs to timeout 
                        if (previousBuild != null && previousBuild.Status == BuildStatus.Failed && string.IsNullOrEmpty(previousBuild.IncidentBuildId))
                        {
                            Console.WriteLine($"Skipping task {task.Id} for buidl {build.Id}, previous build {previousBuild.Id} is marked as fail but doesn't yet have an incident assigned");
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
                    }
                    catch (Exception ex) 
                    {
                        task.HasPassed = false;
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.Result = ex.ToString();
                        dataLayer.SaveDaemonTask(task);
                    }
                }
            }
            finally
            {
                activeItems.Clear(this);
            }
        }

        #endregion
    }
}
