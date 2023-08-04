using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildPostProcessDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonProcessRunner _processRunner;

        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly BuildEventHandlerHelper _buildLevelPluginHelper;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public BuildPostProcessDaemon(ILogger log, IDaemonProcessRunner processRunner)
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
            IDataPlugin dataRead = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<DaemonTask> tasks = dataRead.GetPendingDaemonTasksByTask((int)DaemonTaskTypes.PostProcess);
            DaemonTaskProcesses daemonProcesses = _di.Resolve<DaemonTaskProcesses>();

            foreach (DaemonTask task in tasks)
            {
                using (IDataPlugin dataWrite = _pluginProvider.GetFirstForInterface<IDataPlugin>()) 
                {
                    try
                    {
                        Build build = dataRead.GetBuildById(task.BuildId);

                        IEnumerable<DaemonTask> blocking = dataRead.DaemonTasksBlocked(build.Id, (int)DaemonTaskTypes.PostProcess);
                        if (blocking.Any())
                        {
                            daemonProcesses.MarkBlocked(task, this, blocking);
                            continue;
                        }

                        daemonProcesses.MarkActive(task, $"Task : {task.Id}, Build {build.Id}");
                        Job job = dataRead.GetJobById(build.JobId);

                        task.HasPassed = true;
                        task.Result = string.Empty;

                        job.PostProcessors.AsParallel().ForAll(delegate (string blamePlugin)
                        {
                            try
                            {
                                IPostProcessorPlugin processor = _pluginProvider.GetByKey(blamePlugin) as IPostProcessorPlugin;

                                PostProcessResult result = processor.Process(build);
                                task.Result += result.Result;
                                if (!result.Passed)
                                    task.HasPassed = false;

                                Console.WriteLine($"Processed build id {build.Id} with plugin {blamePlugin}");
                            }
                            catch (Exception ex)
                            {
                                _log.LogError($"Unexpected error trying to blame build id \"{build.Id}\" with blame \"{blamePlugin}\" : {ex}");
                                task.HasPassed = false;
                                if (task.Result == null)
                                    task.Result = string.Empty;

                                task.Result = $"{task.Result}\n{ex}";
                            }
                        });

                        dataWrite.TransactionStart();
                        task.ProcessedUtc = DateTime.UtcNow;
                        dataWrite.SaveDaemonTask(task);
                        dataWrite.TransactionCommit();
                        daemonProcesses.MarkDone(task);
                    }
                    catch (WriteCollisionException ex)
                    {
                        dataWrite.TransactionCancel();
                        _log.LogWarning($"Write collision trying to process task {task.Id}, trying again later: {ex}");
                    }
                    catch (Exception ex)
                    {
                        dataWrite.TransactionCancel();

                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = false;
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
