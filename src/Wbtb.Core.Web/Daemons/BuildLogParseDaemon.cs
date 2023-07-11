using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
                int activeTaskCount = 0;
                foreach (DaemonTask task in tasks) 
                {
                    Build build = dataLayer.GetBuildById(task.BuildId);
                    if (dataLayer.DaemonTasksBlocked(build.Id, TaskGroup).Any())
                        continue;

                    Job job = dataLayer.GetJobById(build.JobId);
                    foreach (string logParser in job.LogParsers) 
                    {
                        string key = $"{logParser}_{build.Id}";
                        if (activeItems.Has(key) || activeTaskCount > _config.MaxThreads)
                            continue;

                        activeItems.Add(key, $"Parsing build {build.Identifier}, job {job.Name} with {logParser}");
                        ILogParserPlugin parserPlugin = _pluginProvider.GetByKey(logParser) as ILogParserPlugin;

                            new Thread(() =>
                            {
                                Thread.CurrentThread.IsBackground = true;
                                try
                                {
                                    lock(this)
                                        activeTaskCount++;

                                    string rawLog = File.ReadAllText(build.LogPath);
                                    parserPlugin.Pickup(rawLog);
                                }
                                catch (Exception ex)
                                {
                                    _log.LogError($"Unexpected error parsing build {build.Identifier}, job {job.Name} with {logParser}", ex);
                                }
                                finally 
                                {
                                    activeItems.Add(key, $"Parsing build {build.Identifier}, job {job.Name} with {logParser}");
                                    lock (this)
                                        activeTaskCount--;
                                }
                            }).Start();
                    }
                }

                // rescan all queued parse jobs, check how many are finsihed
                //tasks.AsParallel().ForAll(delegate (DaemonTask task)
                foreach(DaemonTask task in tasks)
                {
                    try
                    {
                        Build build = dataLayer.GetBuildById(task.BuildId);
                        activeItems.Add(this, $"Task : {task.Id}, Build {build.Id}");

                        if (dataLayer.DaemonTasksBlocked(build.Id, TaskGroup).Any())
                            return;

                        Job job = dataLayer.GetJobById(build.JobId);

                        task.HasPassed = true;
                        task.Result = string.Empty;

                        foreach (string logParserPlugin in job.LogParsers) 
                        {
                            try
                            {
                                ILogParserPlugin parser = _pluginProvider.GetByKey(logParserPlugin) as ILogParserPlugin;

                                string rawLog = File.ReadAllText(build.LogPath);

                                BuildLogParseResult logParserResult = new BuildLogParseResult();
                                logParserResult.BuildId = build.Id;
                                logParserResult.LogParserPlugin = parser.ContextPluginConfig.Key;
                                logParserResult.ParsedContent = string.Empty;

                                DateTime startUtc = DateTime.UtcNow;
                                LogParsePickupResult pickup = parser.Pickup(rawLog);
                                if (!pickup.Found)
                                    continue;

                                logParserResult.ParsedContent = pickup.Result;
                                string timestring = $" took {(DateTime.UtcNow - startUtc).ToHumanString(shorten: true)}";

                                dataLayer.SaveBuildLogParseResult(logParserResult);
                                _log.LogInformation($"Parsed log for build id {build.Id} with plugin {logParserResult.LogParserPlugin}{timestring}");
                                task.Result += $"{logParserResult.LogParserPlugin} {timestring}. ";
                            }
                            catch (Exception ex)
                            {
                                _log.LogError($"Unexpected error trying to process jobs/logs for build id \"{build.Id}\" with lopParserPlugin \"{logParserPlugin}\" : {ex}");
                                task.HasPassed = false;
                                if (task.Result == null)
                                    task.Result = string.Empty;

                                task.Result = $"{task.Result}\n Unexpected error trying to process jobs/logs for build id \"{build.Id}\" with lopParserPlugin \"{logParserPlugin}\": {ex.Message}";
                            }
                        };
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
            finally
            {
                activeItems.Clear(this);
            }
        }

        #endregion
    }
}
