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
            IDataPlugin dataRead = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<DaemonTask> tasks = dataRead.GetPendingDaemonTasksByTask((int)DaemonTaskTypes.LogParse).Take(_config.MaxThreads);
            DaemonTaskProcesses daemonProcesses = _di.Resolve<DaemonTaskProcesses>();

            // start as many parallel parses as we're allowed, on bg threads
            // foreach (DaemonTask task in tasks) 
            tasks.AsParallel().ForAll(delegate (DaemonTask task)
            {
                using (IDataPlugin dataWrite = _pluginProvider.GetFirstForInterface<IDataPlugin>()) 
                {
                    Build build = dataRead.GetBuildById(task.BuildId);
                    IEnumerable<DaemonTask> blocking = dataRead.DaemonTasksBlocked(build.Id, (int)DaemonTaskTypes.LogParse);
                    if (blocking.Any())
                    {
                        daemonProcesses.MarkBlocked(task, this, blocking);
                        return;
                    }

                    Job job = dataRead.GetJobById(build.JobId);
                    try
                    {
                        dataWrite.TransactionStart();

                        ILogParserPlugin parser = _pluginProvider.GetByKey(task.Args) as ILogParserPlugin;
                        if (parser == null)
                        {
                            task.HasPassed = false;
                            task.Result += $"Log parser {task.Args} was not found.";
                            task.ProcessedUtc = DateTime.UtcNow;
                            dataWrite.SaveDaemonTask(task);
                            dataWrite.TransactionCommit();
                            daemonProcesses.MarkDone(task);
                            return;
                        }

                        daemonProcesses.MarkActive(task, $"Task {task.Id}, build {build.Id}, parser {parser.ContextPluginConfig.Manifest.Key}");

                        // todo : optimize, have to reread log just to hash is a major performance issue
                        string rawLog = File.ReadAllText(build.LogPath);
                        DateTime startUtc = DateTime.UtcNow;
                        string result = parser.Parse(rawLog);

                        BuildLogParseResult logParserResult = new BuildLogParseResult();
                        logParserResult.BuildId = build.Id;
                        logParserResult.LogParserPlugin = parser.ContextPluginConfig.Key;
                        logParserResult.ParsedContent = result;
                        dataWrite.SaveBuildLogParseResult(logParserResult);

                        string timestring = $" took {(DateTime.UtcNow - startUtc).ToHumanString(shorten: true)}";
                        _log.LogInformation($"Parsed log for build id {build.Id} with plugin {logParserResult.LogParserPlugin}{timestring}");
                        task.Result = $"{logParserResult.LogParserPlugin} {timestring}. ";
                        task.ProcessedUtc = DateTime.UtcNow;
                        task.HasPassed = true;
                        dataWrite.SaveDaemonTask(task);
                        dataWrite.TransactionCommit();
                        daemonProcesses.MarkDone(task);
                    }
                    catch (WriteCollisionException ex)
                    {
                        dataWrite.TransactionCancel();
                        _log.LogWarning($"Write collision trying to process task {task.Id}, trying again later");
                    }
                    catch (Exception ex)
                    {
                        dataWrite.TransactionCancel();

                        _log.LogError($"Unexpected error trying to process jobs/logs for build id \"{build.Id}\" with lopParserPlugin : {ex}");
                        task.HasPassed = false;
                        task.Result = $"{task.Result}\n Unexpected error trying to process jobs/logs for build id \"{build.Id}\" with lopParserPlugin: {ex.Message}";
                        task.ProcessedUtc = DateTime.UtcNow;
                        dataWrite.SaveDaemonTask(task);
                        daemonProcesses.MarkDone(task);
                    }
                    finally
                    {
                        daemonProcesses.ClearActive(task);
                    }
                } // using
            });
        }

        #endregion
    }
}
