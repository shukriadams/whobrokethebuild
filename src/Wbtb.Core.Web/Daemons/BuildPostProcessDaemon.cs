﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

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

        public static int TaskGroup = 4;

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
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.PostProcess.ToString());
            DaemonActiveProcesses activeItems = _di.Resolve<DaemonActiveProcesses>();

            try
            {
                foreach (DaemonTask task in tasks)
                {
                    try
                    {
                        Build build = dataLayer.GetBuildById(task.BuildId);
                        if (dataLayer.DaemonTasksBlocked(build.Id, TaskGroup).Any())
                            continue;

                        activeItems.Add(this, $"Task : {task.Id}, Build {build.Id}");
                        Job job = dataLayer.GetJobById(build.JobId);
                        
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

                        task.ProcessedUtc = DateTime.UtcNow;
                        dataLayer.SaveDaemonTask(task);
                    }
                    catch (Exception ex)
                    {
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = false;
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
