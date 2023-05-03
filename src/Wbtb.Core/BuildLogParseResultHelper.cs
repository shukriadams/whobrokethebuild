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
        public void ProcessBuild(IDataLayerPlugin dataLayer, Build buildWithUnparsedLogs, ILogParser parser, ILogger log)
        {
            try
            {
                string rawLog = File.ReadAllText(buildWithUnparsedLogs.LogPath);

                BuildLogParseResult logParserResult = new BuildLogParseResult();
                logParserResult.ParsedContent = parser.Parse(rawLog);
                logParserResult.BuildId = buildWithUnparsedLogs.Id;
                logParserResult.LogParserPlugin = parser.ContextPluginConfig.Key;

                dataLayer.SaveBuildLogParseResult(logParserResult);
                Console.WriteLine($"Parsed log for build id {buildWithUnparsedLogs.Id} with plugin {logParserResult.LogParserPlugin}");
            }
            catch (Exception ex)
            {
                log.LogError($"Unexpected error trying to parse logs for build \"{buildWithUnparsedLogs.Id}\" : {ex}");

                dataLayer.SaveBuildFlag(new BuildFlag
                {
                    BuildId = buildWithUnparsedLogs.Id,
                    Description = $"error parsing logs: {ex}",
                    Flag = BuildFlags.LogParseFailed
                });
            }
        }
    }
}
