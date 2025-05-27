using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Common runner logic for all daemons. Responsible for ticking each daemon on its own thread, and that ticks do not collide. Per tick, daemons
    /// fulfill daemontasks, these can be executed on their own threads.
    /// 
    /// Each instance of a daemon has its own process runner.
    /// </summary>
    public class DaemonTaskController : IDaemonTaskController
    {
        #region FIELDS

        /// <summary>
        /// 
        /// </summary>
        private bool _running;

        private bool _busy;

        #endregion

        #region CTORS

        public DaemonTaskController()
        {
            _running = true;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Starts a daemon that runs child processes on its own thread.
        /// </summary>
        /// <param name="daemon">Daemon this controller will be ticking.</param>
        /// <param name="tickInterval"></param>
        public void WatchForAndRunTasksForDaemon(IWebDaemon daemon, int tickInterval)
        {
            // do each work tick on its own thread
            new Thread(delegate ()
            {
                while (_running)
                {
                    if (_busy)
                        return;

                    _busy = true;

                    try
                    {
                        daemon.Work();
                    }
                    finally
                    {
                        _busy = false;
                    }

                    Thread.Sleep(tickInterval);
                }
            }).Start();
        }

        /// <summary>
        /// Watches for tasks for the given daemonLevel, then executes them on work delegate
        /// </summary>
        /// <param name="work">Delegate from daemon implementation that will be executed to do work on task. Delegate is used instead of calling method directory so daemon can implement whatever interface it wants  </param>
        /// <param name="tickIntervalMilliseconds">Sleep time between ticks where tasks are checked for etc.</param>
        /// <param name="daemon">Daemon this controller will be ticking.</param>
        /// <param name="daemonLevel"></param>
        public void WatchForAndRunTasksForDaemon(IWebDaemon daemon, int tickIntervalMilliseconds, ProcessStages? daemonLevel)
        {
            int thisTaskLevel = 9999; // pick an absurdly high number to ensure we overshoot
            if (daemonLevel.HasValue)
                thisTaskLevel = (int)daemonLevel;

            // run forever loop on own background thread 
            new Thread(delegate ()
            {
                while (_running)
                {
                    if (_busy)
                        return;

                    _busy = true;

                    try
                    {
                        SimpleDI di = new SimpleDI();
                        DaemonTaskProcesses daemonProcesses = di.Resolve<DaemonTaskProcesses>();
                        PluginProvider pluginProvider = di.Resolve<PluginProvider>();
                        IDataPlugin dataRead = pluginProvider.GetFirstForInterface<IDataPlugin>();
                        Configuration configuration = di.Resolve<Configuration>();
                        ILogger log = di.Resolve<ILogger>();

                        IEnumerable<DaemonTask> tasks = dataRead.GetPendingDaemonTasksByLevel(thisTaskLevel);
                        
                        foreach (DaemonTask task in tasks)
                        {
                            // check if threads are available, if not, drop all tasks and wait for next controller tick
                            try
                            {
                                if (daemonProcesses.GetAllActive().Count() >= configuration.MaxThreads)
                                    break;

                                if (daemonProcesses.GetAllActive().Where(t => t.Daemon == this.GetType()).Count() >= configuration.MaxThreadsPerDaemon)
                                    break;
                            }
                            catch (Exception)
                            {
                                // the code above is _very_ unstable, getting plenty of thread exceptions that I can't figure out.
                                // cross thread error on active collections, assume too many processes and try again late
                                break;
                            }

                            DaemonActiveProcess activeProcess = daemonProcesses.GetActive(task);
                            if (activeProcess != null)
                            {
                                // Task is already running

                                // Check if running task has timed out, if so, mark it as failed
                                if ((DateTime.UtcNow - activeProcess.CreatedUtc).TotalSeconds > configuration.DaemonTaskTimeout)
                                {
                                    task.ProcessedUtc = DateTime.UtcNow;
                                    task.HasPassed = false;
                                    task.AppendResult($"Marked as timed out after {configuration.DaemonTaskTimeout} seconds.");

                                    try
                                    {
                                        dataRead.SaveDaemonTask(task);
                                        daemonProcesses.MarkDone(task);
                                    }
                                    catch (WriteCollisionException) 
                                    {
                                        log.LogWarning($"Write collision trying to mark timed out task id {task.Id}, trying again later Ignored unless flooding.");
                                    }
                                }

                                continue;
                            }

                            Build build = dataRead.GetBuildById(task.BuildId);
                            IEnumerable<DaemonTask> blockedTasksForThisBuild = dataRead.GetBlockedDaemonTasks(build.Id, thisTaskLevel);
                            IEnumerable<DaemonTask> failedTasksForThisBuild = blockedTasksForThisBuild.Where(t => t.HasPassed.HasValue && !t.HasPassed.Value);
                            
                            // if previous fails in build, mark this as failed to.
                            if (failedTasksForThisBuild.Any())
                            {
                                task.ProcessedUtc = DateTime.UtcNow;
                                task.HasPassed = false;
                                task.AppendResult($"Marked as failed because preceding task(s) failed. Fix then rereset job id {build.JobId}.");
                                string failingTaskId = failedTasksForThisBuild.First().Id;
                                
                                if (failedTasksForThisBuild.Any(f => string.IsNullOrEmpty(f.FailDaemonTaskId)))
                                    failingTaskId = failedTasksForThisBuild.Where(f => string.IsNullOrEmpty(f.FailDaemonTaskId)).First().Id;

                                task.FailDaemonTaskId = failingTaskId;
                                
                                try
                                {
                                    dataRead.SaveDaemonTask(task);
                                    daemonProcesses.MarkDone(task);
                                }
                                catch (WriteCollisionException)
                                {
                                    log.LogWarning($"Write collision trying to mark task id {task.Id} as failed, trying again later. Ignored unless flooding.");
                                }

                                continue;
                            }

                            if (blockedTasksForThisBuild.Any())
                            {
                                daemonProcesses.MarkBlocked(task, daemon, build, blockedTasksForThisBuild);
                                continue;
                            }


                            // mark task as active so we don't rerun it                            
                            if (daemonProcesses.IsActive(task))
                                continue;

                            daemonProcesses.MarkActive(task, daemon, build);

                            // do the work tasks requires, but on its own thread so the work doesn't block this controller.
                            new Thread(delegate (){

                                using (IDataPlugin dataWrite = pluginProvider.GetFirstForInterface<IDataPlugin>())
                                {
                                    try
                                    {
                                        Job job = dataRead.GetJobById(build.JobId);

                                        dataWrite.TransactionStart();


                                        // WORK IS DONE HERE - work either passes and task can be marked as done,
                                        // or it fails and it's still marked as done but failing done (requiring manual restart)
                                        // or it's forced to exit from a block, in which case, marked as blocked
                                        DaemonTaskWorkResult workResult = daemon.WorkThreaded(dataRead, dataWrite, task, build, job);


                                        if (workResult.ResultType == DaemonTaskWorkResultType.Passed)
                                        {
                                            // note : task.Result can be set by daemon, don't overwrite it
                                            task.HasPassed = true;
                                            task.ProcessedUtc = DateTime.UtcNow;
                                            task.AppendResult(workResult.Description);
                                            dataWrite.SaveDaemonTask(task);
                                            dataWrite.TransactionCommit();

                                            daemonProcesses.MarkDone(task);

                                        }
                                        else if (workResult.ResultType == DaemonTaskWorkResultType.Blocked)
                                        {
                                            dataWrite.TransactionCancel();
                                            daemonProcesses.MarkBlocked(task, daemon, build, workResult.Description);
                                        }
                                        else if (workResult.ResultType == DaemonTaskWorkResultType.WriteCollision)
                                        {
                                            dataWrite.TransactionCancel();
                                            log.LogWarning($"Write collision occurred trying to update {task.Id}, trying again in a while. Ignored unless flooding.");
                                        }
                                        else if (workResult.ResultType == DaemonTaskWorkResultType.Failed)
                                        {
                                            // task fail is a normal outcome, and happens when some logical condition prevents
                                            // the task from "passing". User intervention is required.

                                            // commit transaction, this exception is normal and expected
                                            task.ProcessedUtc = DateTime.UtcNow;
                                            task.HasPassed = false;
                                            task.AppendResult(workResult.Description);

                                            dataWrite.SaveDaemonTask(task);
                                            dataWrite.TransactionCommit();

                                            daemonProcesses.MarkDone(task);
                                        }

                                        // force readback to ensure the data we just wrote can be read again, this is most likely
                                        // unnecessary but we need to be sure because when this task is marked as done, dependent tasks
                                        // need to be able to query it to do their work.
                                        if (task.ProcessedUtc.HasValue) 
                                        {
                                            while (true)
                                            {
                                                DaemonTask testRead = dataRead.GetDaemonTaskById(task.Id);
                                                if (testRead == null || testRead.ProcessedUtc.HasValue)
                                                    break;

                                                Thread.Sleep(500);
                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        log.LogError($"Unexpected error, daemon {daemon.GetType().Name}, task id {task.Id} ", ex);
 
                                        dataWrite.TransactionCancel();
                                        
                                        try
                                        {
                                            task.ProcessedUtc = DateTime.UtcNow;
                                            task.HasPassed = false;
                                            task.AppendResult(ex);
                                            dataWrite.SaveDaemonTask(task);
                                        }
                                        catch (WriteCollisionException ex2)
                                        {
                                            log.LogError($"nested WriteCollisionException updating error on task {task.Id}, ignoring this and trying again.", ex2);
                                        }

                                        daemonProcesses.MarkDone(task);
                                    }
                                }
                            }).Start();

                        } // got each task
                    }
                    finally
                    {
                        _busy = false;
                    }

                    Thread.Sleep(tickIntervalMilliseconds);
                }
            }).Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _running = false;
        }

        #endregion
    }
}
