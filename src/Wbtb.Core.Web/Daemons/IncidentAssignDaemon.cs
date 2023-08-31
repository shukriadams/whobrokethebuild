using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

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
            _processRunner.Start(new DaemonWorkThreaded(this.WorkThreaded), tickInterval, this, DaemonTaskTypes.IncidentAssign);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _processRunner.Dispose();
        }

        private DaemonTaskWorkResult WorkThreaded(IDataPlugin dataRead, IDataPlugin dataWrite, DaemonTask task, Build build, Job job)
        {
            Build previousBuild = build;

            if (build.Status != BuildStatus.Failed)
                return new DaemonTaskWorkResult { Description = "Build is not failing, does not need incident" };

            // look for a valid previous build in timeline. Previous build must be completed, and must be either pass|fail. If 
            // no build can be found, treat this task as blocked
            while (true) 
            {
                previousBuild = dataRead.GetPreviousBuild(previousBuild);

                if (previousBuild == null)
                {
                    // this build is very first build in job, mark it as the incident build
                    build.IncidentBuildId = build.Id;
                    dataWrite.SaveBuild(build);
                    return new DaemonTaskWorkResult { };
                }

                if (!previousBuild.EndedUtc.HasValue)
                    return new DaemonTaskWorkResult { 
                        ResultType= DaemonTaskWorkResultType.Blocked , 
                        Description = $"Previous build {previousBuild.Id} found, but does not have a status."};

                if (previousBuild.Status == BuildStatus.Failed || previousBuild.Status == BuildStatus.Passed)
                    break;
            }

            if (previousBuild.Status == BuildStatus.Passed)
            {
                // this build is the first build of sequence to fail,
                // mark it as the incident build
                build.IncidentBuildId = build.Id;
                dataWrite.SaveBuild(build);
                return new DaemonTaskWorkResult { };
            }

            // previous build failed, but it's incident hasn't been assigned, wait
            // todo : add some kind of max-tries here, it needs to timeout 
            // todo : this is a critical bottleneck
            if (previousBuild.Status == BuildStatus.Failed && string.IsNullOrEmpty(previousBuild.IncidentBuildId))
            {
                // check to see if the upstream task is marked as failed. If it has, this task is permanently blocked
                // and should be marked as failed. Fixing of upstream task + resetting job necessary.
                DaemonTask previousBuildFailingTask = dataRead.GetDaemonsTaskByBuild(previousBuild.Id)
                    .Where(b => b.HasPassed.HasValue && b.HasPassed.Value == false)
                    .FirstOrDefault();

                if (previousBuildFailingTask != null)
                    return new DaemonTaskWorkResult { ResultType=DaemonTaskWorkResultType.Failed, Description = $"Failed because previous build id:{previousBuild.Id} failed at daemontask id ${previousBuildFailingTask.Id}. Fix upstream task, then reset job." };

                // if reach here, previous build's incident not assigned, but likely because it hasn't been processed yet. 
                // Mark current as blocked and wait.
                Console.WriteLine($"Skipping task {task.Id} for build {build.Id}, previous build {previousBuild.Id} is marked as fail but doesn't yet have an incident assigned");
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Blocked, Description = $"Previous build id {build.Id}, job id {job.Id} waiting for incident assignment" };
            }

            // happy outcome - set incident to whatever previous build incident is 
            build.IncidentBuildId = previousBuild.IncidentBuildId;
            dataWrite.SaveBuild(build);

            return new DaemonTaskWorkResult();
        }

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        private void Work()
        {
            IDataPlugin dataRead = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<DaemonTask> tasks = dataRead.GetPendingDaemonTasksByTask((int)DaemonTaskTypes.IncidentAssign);
            DaemonTaskProcesses daemonProcesses = _di.Resolve<DaemonTaskProcesses>();

            foreach (DaemonTask task in tasks)
            {
                using (IDataPlugin dataWrite = _pluginProvider.GetFirstForInterface<IDataPlugin>())
                {
                    try
                    {
                        Build build = dataRead.GetBuildById(task.BuildId);

                        IEnumerable<DaemonTask> blocking = dataRead.DaemonTasksBlocked(build.Id, (int)DaemonTaskTypes.IncidentAssign);
                        if (blocking.Any())
                        {
                            daemonProcesses.MarkBlocked(task, this, build, blocking);
                            continue;
                        }

                        dataWrite.TransactionStart();

                        daemonProcesses.MarkActive(task, this, build);
                        Build previousBuild = dataRead.GetPreviousBuild(build);
                        if (previousBuild == null || previousBuild.Status == BuildStatus.Passed)
                        {
                            // this build is either the very first build in job and has failed (way to start!) or is the first build of sequence to fail,
                            // mark it as the incident build
                            build.IncidentBuildId = build.Id;
                            dataWrite.SaveBuild(build);

                            task.HasPassed = true;
                            task.ProcessedUtc = DateTime.UtcNow;
                            dataWrite.SaveDaemonTask(task);
                            dataWrite.TransactionCommit();
                            daemonProcesses.MarkDone(task);
                            continue;
                        }

                        // previous build failed, but it's incident hasn't been assigned, wait
                        // todo : add some kind of max-tries here, it needs to timeout 
                        // todo : this is a critical bottleneck
                        if (previousBuild != null && previousBuild.Status == BuildStatus.Failed && string.IsNullOrEmpty(previousBuild.IncidentBuildId))
                        {
                            DaemonTask previousBuildFailingTask = dataRead.GetDaemonsTaskByBuild(previousBuild.Id).Where(b => b.HasPassed.HasValue && b.HasPassed.Value == false).FirstOrDefault();

                            if (previousBuildFailingTask != null) 
                            {
                                task.HasPassed = false;
                                task.ProcessedUtc = DateTime.UtcNow;
                                task.Result = $"Failed because previous build id:{previousBuild.Id} failed at daemontask id ${previousBuildFailingTask.Id}";
                                dataWrite.SaveDaemonTask(task);
                                dataWrite.TransactionCommit();
                                daemonProcesses.MarkDone(task);
                                continue;
                            }

                            Console.WriteLine($"Skipping task {task.Id} for build {build.Id}, previous build {previousBuild.Id} is marked as fail but doesn't yet have an incident assigned");
                            daemonProcesses.MarkBlocked(task, this, build, $"Previous build {build.Id} waiting for incident assignment");
                            dataWrite.TransactionCancel();
                            continue;
                        }

                        // set incident to whatever previous build incident is, check above ensures that if prev failed, it has an incdidentid 
                        if (previousBuild != null)
                        {
                            build.IncidentBuildId = previousBuild.IncidentBuildId;
                            dataWrite.SaveBuild(build);

                            task.HasPassed = true;
                            task.ProcessedUtc = DateTime.UtcNow;
                            dataWrite.SaveDaemonTask(task);
                            dataWrite.TransactionCommit();
                            daemonProcesses.MarkDone(task);
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

                        dataWrite.SaveDaemonTask(task);
                        daemonProcesses.MarkDone(task);

                        dataWrite.TransactionCommit();
                    }
                    catch (WriteCollisionException ex)
                    {
                        dataWrite.TransactionCancel();
                        _log.LogWarning($"Write collision trying to process task {task.Id}, trying again later: {ex}");
                    }
                    catch (Exception ex)
                    {
                        dataWrite.TransactionCancel();

                        task.HasPassed = false;
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.Result = ex.ToString();
                        dataWrite.SaveDaemonTask(task);
                        daemonProcesses.MarkDone(task);
                    }
                    finally
                    {
                        daemonProcesses.ClearActive(task);
                    }
                }

            }
        }

        #endregion
    }
}
