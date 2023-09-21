using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Common runner logic for all daemons. Is reponsible for ticking each daemon on its own thread, and that ticks do not collide. Per tick, daemons
    /// fullfill daemontasks, these can be executed on their own threads.
    /// 
    /// Each instance of a daemon has its own process runniner.
    /// </summary>
    public class DaemonProcessRunner : IDaemonProcessRunner
    {
        /// <summary>
        /// 
        /// </summary>
        private bool _running;

        private bool _busy;

        public DaemonProcessRunner()
        {
            _running = true;
        }

        /// <summary>
        /// Starts a daemon that runs child processes on its own thread.
        /// </summary>
        /// <param name="work"></param>
        /// <param name="tickInterval"></param>
        public void Start(DaemonWork work, int tickInterval)
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
                        work();
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
        /// Starts a daemon that runs child processes on their own threads.
        /// </summary>
        /// <param name="work"></param>
        /// <param name="tickInterval"></param>
        /// <param name="daemon"></param>
        /// <param name="daemonLevel"></param>
        public void Start(DaemonWorkThreaded work, int tickInterval, IWebDaemon daemon, DaemonTaskTypes? daemonLevel)
        {
            int daemonLevelRaw = 9999; // pick an absurdly high number to ensure we overshoot
            if (daemonLevel.HasValue)
                daemonLevelRaw = (int)daemonLevel;

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
                        PluginProvider pluginProvider = di.Resolve<PluginProvider>();
                        IDataPlugin dataRead = pluginProvider.GetFirstForInterface<IDataPlugin>();
                        DaemonTaskProcesses daemonProcesses = di.Resolve<DaemonTaskProcesses>();
                        Configuration configuration = di.Resolve<Configuration>();
                        ILogger log = di.Resolve<ILogger>();

                        IEnumerable<DaemonTask> tasks = dataRead.GetPendingDaemonTasksByTask(daemonLevelRaw);
                        foreach (DaemonTask task in tasks)
                        {
                            try
                            {
                                if (daemonProcesses.GetAllActive().Count() >= configuration.MaxThreads)
                                    break;

                                if (daemonProcesses.GetAllActive().Where(t => t.Daemon == this.GetType()).Count() >= configuration.MaxThreadsPerDaemon)
                                    break;
                            }
                            catch (Exception)
                            {
                                // this section code above is pure unstable trash
                                // cross thread error on active collections, assume too many processes and try again late
                                break;
                            }

                            DaemonActiveProcess activeProcess = daemonProcesses.GetActive(task);
                            if (activeProcess != null)
                            {
                                // task is already running
                                
                                // check if active task has timed out, if so, mark it as failed
                                if ((DateTime.UtcNow - activeProcess.CreatedUtc).TotalSeconds > configuration.DaemonTaskTimeout)
                                {
                                    task.ProcessedUtc = DateTime.UtcNow;
                                    task.HasPassed = false;
                                    task.Result = $"Marked as timed out after {configuration.DaemonTaskTimeout} seconds.";

                                    try
                                    {
                                        dataRead.SaveDaemonTask(task);
                                        daemonProcesses.MarkDone(task);
                                    }
                                    catch (WriteCollisionException) 
                                    {
                                        log.LogWarning($"Write collision trying to mark timedout task id {task.Id}, trying again later Ignored unless flooding.");
                                    }
                                }

                                continue;
                            }

                            Build build = dataRead.GetBuildById(task.BuildId);
                            IEnumerable<DaemonTask> blocking = dataRead.DaemonTasksBlocked(build.Id, daemonLevelRaw);
                            IEnumerable<DaemonTask> failing = blocking.Where(t => t.HasPassed.HasValue && !t.HasPassed.Value);
                            
                            // if previous fails in build, mark this as failed to.
                            if (failing.Any())
                            {
                                task.ProcessedUtc = DateTime.UtcNow;
                                task.HasPassed = false;
                                task.Result = $"Marked as failed because preceeding task(s) failed. Fix then rereset job id {build.JobId}.";
                                string failingTaskId = failing.First().Id;
                                
                                if (failing.Any(f => string.IsNullOrEmpty(f.FailDaemonTaskId)))
                                    failingTaskId = failing.Where(f => string.IsNullOrEmpty(f.FailDaemonTaskId)).First().Id;

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

                            // if no blocks are marked as fails, wait
                            if (blocking.Any())
                            {
                                daemonProcesses.MarkBlocked(task, daemon, build, blocking);
                                continue;
                            }

                            // do each work tick on its own thread
                            new Thread(delegate ()
                            {
                                using (IDataPlugin dataWrite = pluginProvider.GetFirstForInterface<IDataPlugin>())
                                {
                                    try
                                    {
                                        Job job = dataRead.GetJobById(build.JobId);

                                        dataWrite.TransactionStart();

                                        // DO WORK HERE - work either passes and task can be marked as done,
                                        // or it fails and it's still marked as done but failing done (requiring manual restart)
                                        // or it's forced to exit from a block, in which case, marked as blocked
                                        DaemonTaskWorkResult workResult = work(dataRead, dataWrite, task, build, job);

                                        if (workResult.ResultType == DaemonTaskWorkResultType.Passed)
                                        {
                                            // note : task.Result can be set by daemon, don't overwrite it
                                            task.HasPassed = true;
                                            task.ProcessedUtc = DateTime.UtcNow;
                                            dataWrite.SaveDaemonTask(task);
                                            daemonProcesses.MarkDone(task);

                                            dataWrite.TransactionCommit();
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
                                            dataWrite.TransactionCommit();

                                            task.ProcessedUtc = DateTime.UtcNow;
                                            task.HasPassed = false;
                                            task.Result = workResult.Description;
                                            dataWrite.SaveDaemonTask(task);

                                            daemonProcesses.MarkDone(task);
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
                                            task.Result = ex.ToString();
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

                            daemonProcesses.MarkActive(task, daemon, build);
                        }
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
        /// 
        /// </summary>
        public void Dispose()
        {
            _running = false;
        }
    }
}
