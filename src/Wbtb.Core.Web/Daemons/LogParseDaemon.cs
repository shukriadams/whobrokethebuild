using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask(DaemonTaskTypes.AssignIncident.ToString());
            foreach (DaemonTask task in tasks)
            {
                Build build = dataLayer.GetBuildById(task.BuildId);
                Job job = dataLayer.GetJobById(build.JobId);

                foreach (string lopParserPlugin in job.LogParserPlugins)
                {
                    ILogParserPlugin parsr = _pluginProvider.GetByKey(lopParserPlugin) as ILogParserPlugin;
                    _buildLogParseResultHelper.ProcessBuild(dataLayer, build, parsr, _log);
                }

                task.ProcessedUtc = DateTime.UtcNow;
                task.HasPassed = true;
                dataLayer.SaveDaemonTask(task);
            }
            
        }

        #endregion
    }
}
