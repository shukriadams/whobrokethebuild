using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Daemons;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Parses build logs to try to find error lines, as well as link these errors to revisions and users where possible.
    /// </summary>
    public class BuildLogParseDaemon : IWebDaemon
    {
        #region FIELDS

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;
        
        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        public static int TaskGroup = 3;
        
        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public BuildLogParseDaemon(ILogger log, Configuration config, PluginProvider pluginProvider, IDaemonProcessRunner processRunner)
        {
            _log = log;
            _processRunner = processRunner;
            _config = config;
            _pluginProvider = pluginProvider;
            _di = new SimpleDI();
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
            DaemonActiveProcesses activeItems = _di.Resolve<DaemonActiveProcesses>();

            try
            {
                // start as many parallel parses as we're allowed, on bg threads
                // foreach (DaemonTask task in tasks) 
                tasks.AsParallel().ForAll(delegate (DaemonTask task)
                {
                    Build build = dataLayer.GetBuildById(task.BuildId);
                    if (dataLayer.DaemonTasksBlocked(build.Id, TaskGroup).Any())
                        return;

                    Job job = dataLayer.GetJobById(build.JobId);
                    try
                    {
                        ILogParserPlugin parser = _pluginProvider.GetByKey(task.Args) as ILogParserPlugin;
                        if (parser == null)
                        {
                            task.HasPassed = false;
                            task.Result += $"Log parser {task.Args} was not found.";
                            task.ProcessedUtc = DateTime.UtcNow;
                            dataLayer.SaveDaemonTask(task);
                        }

                        // todo : optimize, have to reread log just to hash is a major performance issue
                        string rawLog = File.ReadAllText(build.LogPath);
                        DateTime startUtc = DateTime.UtcNow;
                        string result = parser.Parse(rawLog);

                        BuildLogParseResult logParserResult = new BuildLogParseResult();
                        logParserResult.BuildId = build.Id;
                        logParserResult.LogParserPlugin = parser.ContextPluginConfig.Key;
                        logParserResult.ParsedContent = result;
                        dataLayer.SaveBuildLogParseResult(logParserResult);

                        string timestring = $" took {(DateTime.UtcNow - startUtc).ToHumanString(shorten: true)}";
                        _log.LogInformation($"Parsed log for build id {build.Id} with plugin {logParserResult.LogParserPlugin}{timestring}");
                        task.Result = $"{logParserResult.LogParserPlugin} {timestring}. ";
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = true;
                        dataLayer.SaveDaemonTask(task);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"Unexpected error trying to process jobs/logs for build id \"{build.Id}\" with lopParserPlugin : {ex}");
                        task.HasPassed = false;
                        task.Result = $"{task.Result}\n Unexpected error trying to process jobs/logs for build id \"{build.Id}\" with lopParserPlugin: {ex.Message}";
                        task.ProcessedUtc = DateTime.UtcNow;
                        dataLayer.SaveDaemonTask(task);
                    }
                });
            }
            finally
            {
                activeItems.Clear(this);
            }
        }

        #endregion
    }
}
