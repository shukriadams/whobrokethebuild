using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Wbtb.Core.Common;

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
        
        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public LogParseDaemon(ILogger log, Configuration config, PluginProvider pluginProvider, IDaemonProcessRunner processRunner)
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
            _processRunner.Start(new DaemonWorkThreaded(this.WorkThreaded), tickInterval, this, DaemonTaskTypes.LogParse);
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
            ILogParserPlugin parser = _pluginProvider.GetByKey(task.Args) as ILogParserPlugin;
            if (parser == null)
                return new DaemonTaskWorkResult { ResultType=DaemonTaskWorkResultType.Failed, Description = $"Log parser {task.Args} requested by this task was not found." };

            if (!build.LogFetched)
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Failed, Description = $"Build id:{build.Id} has no log path value." };

            string logPath = Build.GetLogPath(_config, job, build);

            if (!File.Exists(logPath))
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Failed, Description = $"Log for Build id:{build.Id} at path:{logPath} does not exist on disk." };

            // todo : optimize, have to reread log just to hash is a major performance issue
            string rawLog;

            try
            {
                rawLog = File.ReadAllText(logPath);
            }
            catch (Exception ex) 
            {
                _log.LogError($"Failed to read log for build id:{build.Id} at path:{logPath}.", ex);
                return new DaemonTaskWorkResult { ResultType = DaemonTaskWorkResultType.Failed, Description = $"Failed to read log for build id:{build.Id} at path:{logPath}. Exception : {ex}" };
            }

            DateTime startUtc = DateTime.UtcNow;
            string result = parser.Parse(build, rawLog);

            BuildLogParseResult logParserResult = new BuildLogParseResult();
            logParserResult.BuildId = build.Id;
            logParserResult.LogParserPlugin = parser.ContextPluginConfig.Key;
            logParserResult.ParsedContent = result;
            dataWrite.SaveBuildLogParseResult(logParserResult);

            string timestring = $" took {(DateTime.UtcNow - startUtc).ToHumanString(shorten: true)}";
            _log.LogInformation($"Parsed log for build id {build.Id} with plugin {logParserResult.LogParserPlugin}{timestring}");
            task.Result = $"{logParserResult.LogParserPlugin} {timestring}. ";

            return new DaemonTaskWorkResult(); 
        }

        #endregion
    }
}
