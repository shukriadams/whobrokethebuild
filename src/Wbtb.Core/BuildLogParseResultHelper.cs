using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    public class BuildLogParseResultHelper
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataLayer"></param>
        /// <param name="buildWithUnparsedLogs"></param>
        /// <param name="parser"></param>
        /// <param name="log"></param>
        public void ProcessBuild(IDataPlugin dataLayer, Build buildWithUnparsedLogs, ILogParserPlugin parser, ILogger log)
        {
                string rawLog = File.ReadAllText(buildWithUnparsedLogs.LogPath);

                BuildLogParseResult logParserResult = new BuildLogParseResult();
                logParserResult.BuildId = buildWithUnparsedLogs.Id;
                logParserResult.LogParserPlugin = parser.ContextPluginConfig.Key;

                logParserResult.ParsedContent = string.Empty;
                // for now, parse only failed logs.
                if (buildWithUnparsedLogs.Status == BuildStatus.Failed)
                    logParserResult.ParsedContent = parser.Parse(rawLog);

                dataLayer.SaveBuildLogParseResult(logParserResult);
                Console.WriteLine($"Parsed log for build id {buildWithUnparsedLogs.Id} with plugin {logParserResult.LogParserPlugin}");
        }
    }
}
