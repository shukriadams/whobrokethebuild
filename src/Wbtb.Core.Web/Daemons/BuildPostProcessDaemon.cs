using Microsoft.Extensions.Logging;
using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildPostProcessDaemon : IWebDaemon
    {
        #region FIELDS

        private readonly ILogger _log;

        private readonly IDaemonTaskController _taskController;

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public BuildPostProcessDaemon(ILogger log, IDaemonTaskController processRunner)
        {
            _log = log;
            _taskController = processRunner;

            _di = new SimpleDI();
            _pluginProvider = _di.Resolve<PluginProvider>();
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _taskController.WatchForAndRunTasksForDaemon(new DaemonWorkThreaded(this.WorkThreaded), tickInterval, this, DaemonTaskTypes.PostProcess);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _taskController.Dispose();
        }

        private DaemonTaskWorkResult WorkThreaded(IDataPlugin dataRead, IDataPlugin dataWrite, DaemonTask task, Build build, Job job)
        {
            task.Result = string.Empty;

            foreach (string postProcessor in job.PostProcessors)
            {
                try
                {
                    IPostProcessorPlugin processor = _pluginProvider.GetByKey(postProcessor) as IPostProcessorPlugin;

                    PostProcessResult result = processor.Process(build);
                    task.Result += result.Result;
                    if (!result.Passed)
                        task.HasPassed = false;

                    ConsoleHelper.WriteLine(this, $"Processed build id {build.Id} with plugin {postProcessor}");
                }
                catch (Exception ex)
                {
                    _log.LogError($"Unexpected post processor error at build id \"{build.Id}\", processor \"{postProcessor}\" : {ex}");
                    task.HasPassed = false;
                    if (task.Result == null)
                        task.Result = string.Empty;

                    task.Result = $"{task.Result}\n{ex}";
                }
            }

            return new DaemonTaskWorkResult { };
        }

        #endregion
    }
}
