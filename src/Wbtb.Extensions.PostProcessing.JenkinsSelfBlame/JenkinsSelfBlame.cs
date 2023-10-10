using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.JenkinsSelfBlame
{
    public class JenkinsSelfBlame : Plugin, IPostProcessorPlugin
    {
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
            IDataPlugin data = pluginProvider.GetFirstForInterface<IDataPlugin>();
            IEnumerable<BuildLogParseResult> logParseResults = data.GetBuildLogParseResultsByBuildId(build.Id);

            foreach (BuildLogParseResult buildLogParseResult in logParseResults)
            {
                ParsedBuildLogText parsedText = BuildLogTextParser.Parse(buildLogParseResult.ParsedContent);
                if (parsedText == null)
                    continue;

                if (parsedText.Type == "Wbtb.Extensions.LogParsing.JenkinsSelfFailing")
                {
                    string summary = string.Empty;
                    foreach (var item in parsedText.Items)
                        foreach (var item2 in item.Items)
                            summary += $"{item2.Content}";

                    data.SaveMutationReport(new MutationReport
                    {
                        IncidentId = build.IncidentBuildId,
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
            }

            return new PostProcessResult
            {
                Passed = true,
                Result = "Nothing found"
            };
        }
    }

}
