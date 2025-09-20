using Wbtb.Core.Common;
using Microsoft.Extensions.Logging;

namespace Wbtb.Extensions.PostProcessing.JenkinsSelfBlame
{
    public class JenkinsSelfBlame : Plugin, IPostProcessorPlugin
    {
        private readonly BuildLogTextParser _buildLogTextParser;

        private readonly Logger _logger;

        public JenkinsSelfBlame(BuildLogTextParser buildLogTextParser, Logger logger)
        {
            _buildLogTextParser = buildLogTextParser;
            _logger = logger;
        }

        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }



        void IPostProcessorPlugin.VerifyJobConfig(Job job)
        {

        }

        PostProcessResult IPostProcessorPlugin.Process(Build build)
        {
            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            ILogger log = di.Resolve<ILogger>();
            IDataPlugin data = pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<BuildLogParseResult> logParseResults = data.GetBuildLogParseResultsByBuildId(build.Id);

            foreach (BuildLogParseResult buildLogParseResult in logParseResults)
            {
                Response<ParsedBuildLogText> parsedTextResponse = _buildLogTextParser.Parse(buildLogParseResult.ParsedContent);
                if (!string.IsNullOrEmpty(parsedTextResponse.Error)) 
                {
                    _logger.Error(this, parsedTextResponse.Error);
                    continue;
                }

                if (parsedTextResponse.Value == null)
                    continue;

                if (parsedTextResponse.Value.Type == "Wbtb.Extensions.LogParsing.JenkinsSelfFailing")
                {
                    string summary = string.Empty;
                    foreach (var item in parsedTextResponse.Value.Items)
                        foreach (var item2 in item.Items)
                            summary += $"{item2.Content}";

                    // a build can have only one mutation report, don't generate if on already exists
                    if (data.GetMutationReportByBuild(build.Id) == null)
                    {
                        data.SaveMutationReport(new MutationReport
                        {
                            IncidentId = build.IncidentBuildId,
                            BuildId = build.Id,
                            MutationId = build.Id,
                            Description = summary,
                            Processor = this.GetType().Name,
                            Summary = $"Build broken by internal Jenkins error",
                            Status = "Break"
                        });

                        return new PostProcessResult
                        {
                            Passed = true,
                            Result = $"Jenkins error found."
                        };
                    }
                    else
                        log.LogWarning($"{TypeHelper.Name(this)} aborted mutation report, another report exists");

                }
            }

            return new PostProcessResult
            {
                Passed = true,
                Result = "Nothing found"
            };
        }
    }

}
