using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wbtb.Core.Common;

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
            IEnumerable<DaemonTask> tasks = dataLayer.GetPendingDaemonTasksByTask((int)DaemonTaskTypes.LogParse).Take(_config.MaxThreads);
            TaskDaemonProcesses daemonProcesses = _di.Resolve<TaskDaemonProcesses>();

            try
            {
                // start as many parallel parses as we're allowed, on bg threads
                // foreach (DaemonTask task in tasks) 
                tasks.AsParallel().ForAll(delegate (DaemonTask task)
                {
                    string processKey = string.Empty;

                    Build build = dataLayer.GetBuildById(task.BuildId);
                    IEnumerable<DaemonTask> blocking = dataLayer.DaemonTasksBlocked(build.Id, (int)DaemonTaskTypes.LogParse);
                    if (blocking.Any()) 
                    {
                        daemonProcesses.TaskBlocked(task, this, blocking);
                        return;
                    }

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
                            daemonProcesses.TaskDone(task);
                        }
                        
                        processKey = $"{this.GetType().Name}_{task.Id}_{parser.ContextPluginConfig.Manifest.Key}";
                        daemonProcesses.AddActive(processKey, $"Task {task.Id}, build {build.Identifier}, parser {parser.ContextPluginConfig.Manifest.Key}");

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
                        daemonProcesses.TaskDone(task);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"Unexpected error trying to process jobs/logs for build id \"{build.Id}\" with lopParserPlugin : {ex}");
                        task.HasPassed = false;
                        task.Result = $"{task.Result}\n Unexpected error trying to process jobs/logs for build id \"{build.Id}\" with lopParserPlugin: {ex.Message}";
                        task.ProcessedUtc = DateTime.UtcNow;
                        dataLayer.SaveDaemonTask(task);
                        daemonProcesses.TaskDone(task);
                    }
                    finally 
                    {
                        daemonProcesses.ClearActive(processKey);
                    }
                });
            }
            finally
            {
                daemonProcesses.ClearActive(this);
            }
        }

        #endregion
    }
}
