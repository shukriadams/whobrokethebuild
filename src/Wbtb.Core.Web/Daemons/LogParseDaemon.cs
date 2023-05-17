﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        private ILogger _log;

        private IDaemonProcessRunner _processRunner;
        
        private readonly PluginProvider _pluginProvider;

        private readonly Config _config;

        private readonly BuildLogParseResultHelper _buildLogParseResultHelper;

        #endregion

        #region CTORS

        public LogParseDaemon(ILogger log, Config config, PluginProvider pluginProvider, BuildLogParseResultHelper buildLogParseResultHelper, IDaemonProcessRunner processRunner)
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
            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            // start daemons - this should be folded into start
            foreach (BuildServer cfgbuildServer in _config.BuildServers)
            {
                BuildServer buildServer = dataLayer.GetBuildServerByKey(cfgbuildServer.Key);
                // note : buildserver can be null if trying to run daemon before auto data injection has had time to run
                if (buildServer == null)
                    continue;

                // why are we trying to reach build server??
                IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                ReachAttemptResult reach = buildServerPlugin.AttemptReach(buildServer);

                int count = 100;
                if (buildServer.ImportCount.HasValue)
                    count = buildServer.ImportCount.Value;

                if (!reach.Reachable)
                {
                    _log.LogError($"Buildserver {buildServer.Key} not reachable, job import aborted {reach.Error}{reach.Exception}");
                    return;
                }

                foreach (Job job in buildServer.Jobs.Where(job => job.LogParserPlugins.Any()))
                {
                    try
                    {
                        Job thisjob = dataLayer.GetJobByKey(job.Key);
                        if (thisjob.ImportCount.HasValue)
                            count = thisjob.ImportCount.Value;

                        // get log parser plugins for job
                        IList<ILogParser> logParsers = new List<ILogParser>();
                        foreach(string lopParserPlugin in thisjob.LogParserPlugins)
                            logParsers.Add(_pluginProvider.GetByKey(lopParserPlugin) as ILogParser);

                        IEnumerable<Build> buildsWithUnparsedLogs = dataLayer.GetUnparsedBuildLogs(thisjob);
                        foreach (Build buildWithUnparsedLogs in buildsWithUnparsedLogs)
                            foreach(ILogParser parser in logParsers)
                                _buildLogParseResultHelper.ProcessBuild(dataLayer, buildWithUnparsedLogs, parser, _log);

                    }
                    catch (Exception ex)
                    {
                        _log.LogError($"Unexpected error trying to import jobs/logs for \"{job.Key}\" from buildserver \"{buildServer.Key}\" : {ex}");
                    }
                }
            }
        }

        #endregion
    }
}