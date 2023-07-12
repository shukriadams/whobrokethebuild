using Wbtb.Core.Common;

namespace Wbtb.Extensions.PostProcessing.AcmeGamesBlamer
{
    public class AcmeGamesBlamer : Plugin, IPostProcessorPlugin
    {
        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        PostProcessResult IPostProcessorPlugin.Process(Build build)
        {
            SimpleDI di = new SimpleDI();
            Configuration config = di.Resolve<Configuration>();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin data = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = data.GetJobById(build.JobId);
            SourceServer sourceServer = null;
            if (!string.IsNullOrEmpty(job.SourceServerId))
                sourceServer = data.GetSourceServerById(job.SourceServerId);
            
            PluginConfig sourceServerPlugin = null;
            if (sourceServer != null)
                sourceServerPlugin = config.Plugins.FirstOrDefault(p => p.Key == sourceServer.Plugin);

            bool isPerforce = sourceServerPlugin != null && (sourceServerPlugin.Manifest.Key == "Wbtb.Extensions.SourceServer.Perforce" || sourceServerPlugin.Manifest.Key == "Wbtb.Extensions.SourceServer.PerforceSandbox");

            IEnumerable < BuildLogParseResult> logParseResults = data.GetBuildLogParseResultsByBuildId(build.Id);
            IEnumerable<BuildInvolvement> buildInvolvements = data.GetBuildInvolvementsByBuild(build.Id);
            IList<Revision> revisions = new List<Revision>();

            foreach (BuildInvolvement buildInvolvement in buildInvolvements.Where(bi => !string.IsNullOrEmpty(bi.RevisionId)))
                revisions.Add(data.GetRevisionById(buildInvolvement.RevisionId));

            if (build.Status != BuildStatus.Failed)
                return new PostProcessResult
                {
                    Passed = true,
                    Result = "Ignoring non-failed build"
                };

            // if log parse results are c++
            // ensure current source control is perforce
            // foreach file in buildinvolvements revisions
            // can we connect revision file path to c++ file
            if (isPerforce) 
            {
                // parse out clientspec from log

                // if clientspec not found, give up blame, we can't link errors in build workspace to changes in revision

                foreach (BuildLogParseResult result in logParseResults)
                {
                    ParsedBuildLogText parsedText = BuildLogTextParser.Parse(result.ParsedContent);
                    if (parsedText == null)
                        continue;

                    if (parsedText.Type != "Wbtb.Extensions.LogParsing.Cpp")
                        continue;

                    foreach (ParsedBuildLogTextLine line in parsedText.Items)
                        foreach (ParsedBuildLogTextLineItem item in line.Items.Where(l => l.Type == "path"))
                            foreach (Revision revision in revisions)
                            {
                                Console.WriteLine("match found");
                            }
                }
            }


            // if log parse results are blueprint
            // can we parse blueprint file path out of error message
            // foreach file in buildinvolvements revisions
            // can we connect revision file path to blueprint file path
            return new PostProcessResult {
                Passed = true,
                Result = "not implemented yet"
            };

            // if log parse results are jenkins
            // mark all users as not involved
            // add "jenkins" as build involvement, mark is cauase


            // if reach here, need to start guessing
            // if build involvement files are all gfx assets, mark user as unlikely to have caused issue
            
            // for build involvements
            // if revision contains cpp changes and error log is cpp, mark user as _possibly_ involved
            
            // note that alert will construct alerts based on blame level
            // if user definitely or possibly blamed, will alert user with logparse results related to that blame
            // will post public alert messages based on log parse results + blamed user

            // if no blames, will construct error message from all log parse results and send a public alert, but nothing to individual users
        }
    }
}
