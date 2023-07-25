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
            if (build.Status != BuildStatus.Failed)
                return new PostProcessResult
                {
                    Passed = true,
                    Result = "Ignoring non-failed build"
                };

            SimpleDI di = new SimpleDI();
            Configuration config = di.Resolve<Configuration>();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin data = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = data.GetJobById(build.JobId);

            if (string.IsNullOrEmpty(job.SourceServerId))
                return new PostProcessResult
                {
                    Passed = true,
                    Result = "Job has no source control server defined. A Perforce server is required"
                };

            SourceServer sourceServer = data.GetSourceServerById(job.SourceServerId);
            PluginConfig sourceServerPlugin = config.Plugins.FirstOrDefault(p => p.Key == sourceServer.Plugin);

            IEnumerable<BuildLogParseResult> logParseResults = data.GetBuildLogParseResultsByBuildId(build.Id);
            IEnumerable<BuildInvolvement> buildInvolvements = data.GetBuildInvolvementsByBuild(build.Id);

            // get all revisions associated with this build
            IList<Revision> revisionsLinkedToBuild = new List<Revision>();
            foreach (BuildInvolvement buildInvolvement in buildInvolvements.Where(bi => !string.IsNullOrEmpty(bi.RevisionId)))
                revisionsLinkedToBuild.Add(data.GetRevisionById(buildInvolvement.RevisionId));

            // if log parse results are c++

            // ensure current source control is perforce
            // foreach file in buildinvolvements revisions
            // can we connect revision file path to c++ file
            bool isPerforce = sourceServerPlugin != null && (sourceServerPlugin.Manifest.Key == "Wbtb.Extensions.SourceServer.Perforce" || sourceServerPlugin.Manifest.Key == "Wbtb.Extensions.SourceServer.PerforceSandbox");
            if (!isPerforce)
                return new PostProcessResult
                {
                    Passed = true,
                    Result = "Build not covered by perforce, ignoring."
                };

            // parse out clientspec from log : note the mandatory <p4-cient-state> wrapper required for clientspec in buildlog
            string rawLog = File.ReadAllText(build.LogPath);
            string regex = @"<p4-cient-state>([\s\S]*?)<p4-cient-state>";
            string hash = Sha256.FromString(regex + rawLog);
            Cache cache = di.Resolve<Cache>();
            string clientLookup = cache.Get(this, hash);
                
            if (clientLookup == null) 
            {
                Match match = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Match(rawLog);
                if (match.Success)
                {
                    clientLookup = match.Groups[1].Value.Trim();
                    cache.Write(this, hash, clientLookup);
                }
            }

            string resultText = string.Empty;
            Client client = null;
            if (clientLookup != null)
                client = PerforceUtils.ParseClient(clientLookup);

            bool causeFound = false;
            bool cppFound = false;
            string blamedUserName = null;
            User blamedUser = null;

            foreach (BuildLogParseResult buildLogParseResult in logParseResults)
            {
                ParsedBuildLogText parsedText = BuildLogTextParser.Parse(buildLogParseResult.ParsedContent);
                if (parsedText == null)
                    continue;

                if (parsedText.Type != "Wbtb.Extensions.LogParsing.Cpp")
                    continue;

                // from here on, only CPP errors are handled
                cppFound = true;

                if (client == null)
                    continue;

                foreach (ParsedBuildLogTextLine line in parsedText.Items)
                    foreach (ParsedBuildLogTextLineItem localPathItem in line.Items.Where(l => l.Type == "path")) 
                    {
                        // force unitpaths, p4 uses these
                        string localFile = localPathItem.Content.Replace("\\", "/");
                        string clientRoot = client.Root.Replace("\\", "/");

                        foreach (Revision revision in revisionsLinkedToBuild)
                            foreach (string revisionFile in revision.Files)
                            {
                                // is revisionFile mentioned in a log error?
                                foreach (ClientView clientView in client.Views) 
                                {
                                    // currently support only standard mapping
                                    if (!clientView.Local.EndsWith("/..."))
                                        continue;

                                    string root = clientView.Local.Substring(0, clientView.Local.Length - 4); // clip off trailing /...;
                                    string localFileMappedToRemote = string.Join("/", localFile.Replace(clientRoot, string.Empty).Split("/", StringSplitOptions.RemoveEmptyEntries).ToArray());
                                    localFileMappedToRemote = clientView.Remote.Substring(0, clientView.Local.Length - 4) + "/" + localFileMappedToRemote;

                                    if (localFileMappedToRemote != revisionFile)
                                        continue;

                                    BuildInvolvement bi = buildInvolvements.FirstOrDefault(bi => bi.RevisionCode == revision.Code);
                                    if (bi == null) 
                                        return new PostProcessResult
                                        {
                                            Passed = false,
                                            Result = $"Could not find a buildinvolvement for revision {revision.Code} in this build."
                                        };

                                    blamedUserName = revision.User;
                                    blamedUser = data.GetUserById(bi.MappedUserId);

                                    bi.BlameScore = 100;
                                    data.SaveBuildInvolement(bi);
                                    resultText += $"File ${revisionFile} from revision {revision.Code} impplicated in break.\n";
                                    causeFound = true;
                                }
                            }
                    }
            }



            if (!cppFound)
                return new PostProcessResult
                {
                    Passed = true,
                    Result = $"No CPP parse resuls found."
                };

            if (client == null)
                return new PostProcessResult
                {
                    Passed = true,
                    Result = $"Could not parse out p4 clientspec from log."
                };

            // if log parse results are blueprint
            // can we parse blueprint file path out of error message
            // foreach file in buildinvolvements revisions
            // can we connect revision file path to blueprint file path
            if (causeFound) 
            {
                data.SaveIncidentSummary(new IncidentSummary
                {
                    IncidentId = build.IncidentBuildId,
                    MutationId = build.Id,
                    Description = $"Found error in P4 revision. {resultText}.",
                    Processor = this.GetType().Name,
                    Summary = "Build broken by p4 change by X",
                    Status = "Break"
                });

                return new PostProcessResult
                {
                    Passed = true,
                    Result = $"Found error in P4 revision. {resultText}."
                };
            }

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
