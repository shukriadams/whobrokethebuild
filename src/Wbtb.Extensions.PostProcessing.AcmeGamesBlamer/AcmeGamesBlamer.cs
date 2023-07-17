using System.Text.RegularExpressions;
using Wbtb.Core.Common;
using Madscience.Perforce;

namespace Wbtb.Extensions.PostProcessing.AcmeGamesBlamer
{
    /// <summary>
    /// Does Perforce + C++ + Unreal Blueprint error checking.
    /// Requires that p4 clientspec is outputted in build log, wrapped between <p4-cient-state>...<p4-cient-state> tags.
    /// </summary>
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

            /*
            // IGNORE FOR NOW; RE-ENABLE
            if (build.Status != BuildStatus.Failed)
                return new PostProcessResult
                {
                    Passed = true,
                    Result = "Ignoring non-failed build"
                };
            */

            // if log parse results are c++
            // ensure current source control is perforce
            // foreach file in buildinvolvements revisions
            // can we connect revision file path to c++ file
            if (!isPerforce)
                return new PostProcessResult
                {
                    Passed = true,
                    Result = "Build not covered by perforce, ignoring."
                };

            // parse out clientspec from log
            string rawLog = File.ReadAllText(build.LogPath);
            string regex = @"<p4-cient-state>([\s\S]*?)<p4-cient-state>";
            string hash = Sha256.FromString(regex + rawLog);
            Cache cache = di.Resolve<Cache>();
            string resultLookup = cache.Get(this, hash);
                
            if (resultLookup == null) 
            {
                Match match = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Match(rawLog);
                if (match.Success)
                {
                    resultLookup = match.Groups[1].Value.Trim();
                    cache.Write(this, hash, resultLookup);
                }
                else 
                {
                    resultLookup = string.Empty;
                }
            }

            string resultText = string.Empty;
            Client client = PerforceUtils.ParseClient(resultLookup);
            bool causeFound = false;
            foreach (BuildLogParseResult buildLogParseResult in logParseResults)
            {
                ParsedBuildLogText parsedText = BuildLogTextParser.Parse(buildLogParseResult.ParsedContent);
                if (parsedText == null)
                    continue;

                if (parsedText.Type != "Wbtb.Extensions.LogParsing.Cpp")
                    continue;

                foreach (ParsedBuildLogTextLine line in parsedText.Items)
                    foreach (ParsedBuildLogTextLineItem localPathItem in line.Items.Where(l => l.Type == "path")) 
                    {
                        // force unitpaths, p4 uses these
                        string localFile = localPathItem.Content.Replace("\\", "/");
                        string clientRoot = client.Root.Replace("\\", "/");

                        foreach (Revision revision in revisions)
                            foreach (string revisionFile in revision.Files)
                            {
                                // is revisionFile in log?
                                foreach (ClientView clientView in client.Views) 
                                {
                                    // currently support only standard mapping
                                    if (!clientView.Local.EndsWith("/..."))
                                        continue;

                                    string root = clientView.Local.Substring(0, clientView.Local.Length - 4); // clip off trailing /...;
                                    string localFileWithoutRoot = string.Join("/", localFile.Replace(clientRoot, string.Empty).Split("/", StringSplitOptions.RemoveEmptyEntries).ToArray());
                                    localFileWithoutRoot = clientView.Remote.Substring(0, clientView.Local.Length - 4) + "/" + localFileWithoutRoot;

                                    if (localFileWithoutRoot == revisionFile) 
                                    {
                                        BuildInvolvement bi = buildInvolvements.FirstOrDefault(bi => bi.RevisionCode == revision.Code);
                                        if (bi == null) 
                                            return new PostProcessResult
                                            {
                                                Passed = false,
                                                Result = $"Could not find a buildinvolvement for revision {revision.Code} in this build."
                                            };

                                        bi.BlameScore = 100;
                                        data.SaveBuildInvolement(bi);
                                        resultText += $"File ${revisionFile} from revision {revision.Code} impplicated in break.\n";
                                        causeFound = true;
                                    }
                                }
                            }
                    }
            }

            // if log parse results are blueprint
            // can we parse blueprint file path out of error message
            // foreach file in buildinvolvements revisions
            // can we connect revision file path to blueprint file path
            if (causeFound)
                return new PostProcessResult {
                    Passed = true,
                    Result = $"Found error in P4 revision. {resultText}."
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
            return new PostProcessResult
            {
                Passed = true,
                Result = "No blame found"
            };

        }
    }
}
