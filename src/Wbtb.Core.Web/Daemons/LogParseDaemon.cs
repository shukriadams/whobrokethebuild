using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Parses build logs to try to find error lines, as well as link these errors to revisions and users where possible.
    /// </summary>
    public class LogParseDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;
        
        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        private readonly BuildLogParseResultHelper _buildLogParseResultHelper;

        public static int TaskGroup = 3;

        #endregion

        #region CTORS

        public LogParseDaemon(ILogger log, Configuration config, PluginProvider pluginProvider, BuildLogParseResultHelper buildLogParseResultHelper, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;
            _buildLogParseResultHelper = buildLogParseResultHelper;
            _config = config;
            _pluginProvider = pluginProvider;
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
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.LogParse.ToString());
            foreach (DaemonTask task in tasks)
            {
                try
                {
                    Build build = dataLayer.GetBuildById(task.BuildId);
                    if (dataLayer.DaemonTasksBlocked(build.Id, TaskGroup))
                        continue;

                    Job job = dataLayer.GetJobById(build.JobId);

                    job.LogParserPlugins.AsParallel().ForAll(delegate (string lopParserPlugin) {
                        try
                        {
                            ILogParserPlugin parser = _pluginProvider.GetByKey(lopParserPlugin) as ILogParserPlugin;
                            _buildLogParseResultHelper.ProcessBuild(dataLayer, build, parser, _log);
                        }
                        catch (Exception ex)
                        {
                            _log.LogError($"Unexpected error trying to process jobs/logs for build id \"{build.Id}\" with lopParserPlugin \"{lopParserPlugin}\" : {ex}");
                        }
                    });

                    task.HasPassed = true;
                }
                catch (Exception ex)
                {
                    task.HasPassed = false;
                    task.Result = ex.ToString();
                }

                task.ProcessedUtc = DateTime.UtcNow;
                dataLayer.SaveDaemonTask(task);

            }

        }

        #endregion
    }
}
