using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class DaemonProcessRunner : IDaemonProcessRunner
    {
        private bool _running;

        private bool _busy;

        public DaemonProcessRunner()
        {
            _running = true;
        }

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
                            if (daemonProcesses.GetAllActive().Count() > configuration.MaxThreads)
                                break;

                            if (daemonProcesses.IsActive(task))
                                continue;

                            Build build = dataRead.GetBuildById(task.BuildId);

                            IEnumerable<DaemonTask> blocking = dataRead.DaemonTasksBlocked(build.Id, daemonLevelRaw);
                            if (blocking.Any())
                            {
                                daemonProcesses.MarkBlocked(task, daemon, build);
                                continue;
                            }

                            daemonProcesses.MarkActive(task, daemon, build);

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
                                        work(dataRead, dataWrite, task, build, job);

                                        // note : task.Result can be set by daemon, don't overwrite it
                                        task.HasPassed = true;
                                        task.ProcessedUtc = DateTime.UtcNow;
                                        dataWrite.SaveDaemonTask(task);
                                        daemonProcesses.MarkDone(task);

                                        dataWrite.TransactionCommit();
                                    }
                                    catch (DaemonTaskBlockedException ex)
                                    {
                                        dataWrite.TransactionCancel();
                                        daemonProcesses.MarkBlocked(task, daemon, build, ex.Message);
                                    }
                                    catch (WriteCollisionException ex)
                                    {
                                        // ignore these, we'll try again later
                                        dataWrite.TransactionCancel();
                                        log.LogWarning($"Write collision occurred trying to update {task.Id}, trying again in a while.");
                                    }
                                    catch (DaemonTaskFailedException ex)
                                    {
                                        // task fail is a normal outcome, and happens when some logical condition prevents
                                        // the task from "passing". User intervention is required.

                                        // commit transaction, this exception is normal and expected
                                        dataWrite.TransactionCommit();

                                        task.ProcessedUtc = DateTime.UtcNow;
                                        task.HasPassed = false;
                                        task.Result = ex.Message; // store message, not stack trace, the message will explicitly explain enough
                                        dataWrite.SaveDaemonTask(task);

                                        daemonProcesses.MarkDone(task);
                                    }
                                    catch (Exception ex)
                                    {
                                        dataWrite.TransactionCancel();

                                        task.ProcessedUtc = DateTime.UtcNow;
                                        task.HasPassed = false;
                                        task.Result = ex.ToString();
                                        dataWrite.SaveDaemonTask(task);

                                        daemonProcesses.MarkDone(task);

                                        log.LogError($"Unexpected error, daemon {daemon.GetType().Name}, task id {task.Id} ", ex);
                                    }
                                }
                            }).Start();
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
