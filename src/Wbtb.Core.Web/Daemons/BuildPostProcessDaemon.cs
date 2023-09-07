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
            _processRunner.Start(new DaemonWorkThreaded(this.WorkThreaded), tickInterval, this, DaemonTaskTypes.PostProcess);
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

                    Console.WriteLine($"Processed build id {build.Id} with plugin {postProcessor}");
                }
                catch (Exception ex)
                {
                    _log.LogError($"Unexpected error trying to blame build id \"{build.Id}\" with blame \"{postProcessor}\" : {ex}");
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
