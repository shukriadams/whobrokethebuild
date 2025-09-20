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
    public class LogParseDaemon : IWebDaemon
    {
        #region FIELDS

        private Logger _log;

        private IDaemonTaskController _taskController;
        
        private readonly PluginProvider _pluginProvider;

        private readonly Configuration _config;

        #endregion

        #region CTORS

        public LogParseDaemon(Logger log, Configuration config, PluginProvider pluginProvider, IDaemonTaskController processRunner)
        {
            _log = log;
            _taskController = processRunner;
            _config = config;
            _pluginProvider = pluginProvider;
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _taskController.WatchForAndRunTasksForDaemon(this, tickInterval, ProcessStages.LogParse);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _taskController.Dispose();
        }

        void IWebDaemon.Work()
        { 
            throw new NotImplementedException();
        }

        DaemonTaskWorkResult IWebDaemon.WorkThreaded(IDataPlugin dataRead, IDataPlugin dataWrite, DaemonTask task, Build build, Job job) 
        {
            ILogParserPlugin parser = _pluginProvider.GetByKey(task.Args) as ILogParserPlugin;
            if (parser == null)
                return new DaemonTaskWorkResult { ResultType=DaemonTaskWorkResultType.Failed, Description = $"Log parser {task.Args} requested by this task was not found." };

            if (!build.LogFetched)
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Failed, Description = $"Build id:{build.Id} has no log path value." };

            string logPath = Build.GetLogPath(_config, job, build);

            if (!File.Exists(logPath))
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Failed, Description = $"Log for Build id:{build.Id} at path:{logPath} does not exist on disk." };

            string rawLog;

            try
            {
                rawLog = File.ReadAllText(logPath);
            }
            catch (Exception ex) 
            {
                _log.Error(this, $"Failed to read log for build id:{build.Id} at path:{logPath}.", ex);
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Failed, Description = $"Failed to read log for build id:{build.Id} at path:{logPath}. Exception : {ex}" };
            }

            string result = string.Empty;

            // force maximum length on logs parsed, this is necessary to prevent extremely long logs from poisoning regexs in parsers.
            if (rawLog.Length > _config.MaxParsableLogSize) 
            {
                result = $"Log length exceeds maximum allowed character length of ${_config.MaxParsableLogSize}. Log will not be parsed. To bypass this, increase \"{nameof(Configuration.MaxParsableLogSize)}\" in config, restart server and reset this build.";
            } 
            else 
            {
                // remove long lines from log before parsing
                IEnumerable<string> lines = rawLog.Split(" ");
                int unfilteredCount = lines.Count();
                lines = lines.Where(line => line.Length < _config.MaxLineLength);
                if (lines.Count() < unfilteredCount)
                    result = $"{(unfilteredCount - lines.Count())} line(s) removed from log parsing because they exceed maximum continuous length of \"{_config.MaxLineLength}\" charachers.";
                rawLog = string.Join("", lines);

                // parse happens here
                result += parser.Parse(build, rawLog);
            }
            DateTime startUtc = DateTime.UtcNow;

            BuildLogParseResult logParserResult = new BuildLogParseResult();
            logParserResult.BuildId = build.Id;
            logParserResult.LogParserPlugin = parser.ContextPluginConfig.Key;
            logParserResult.ParsedContent = result;
            dataWrite.SaveBuildLogParseResult(logParserResult);

            string timeString = $" took {(DateTime.UtcNow - startUtc).ToHumanString(shorten: true)}";
            _log.Debug(this, $"Parsed log for build id {build.Id} with plugin {logParserResult.LogParserPlugin}{timeString}");
            task.AppendResult($"{logParserResult.LogParserPlugin} {timeString}.");

            _log.Status(this, $"Log parsed for build {build.Key} (id:{build.Id})");
            return new DaemonTaskWorkResult(); 
        }

        #endregion
    }
}
