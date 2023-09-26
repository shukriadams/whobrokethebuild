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
        
        void IPostProcessorPlugin.VerifyJobConfig(Job job) 
        {
            if (job.Config == null)
                throw new ConfigurationException($"Job {job.Name} missing \"Config\" node.");

            if (!job.Config.Any(c => c.Key == "GameRoot") && job.PostProcessors.Contains(this.ContextPluginConfig.Key))
                throw new ConfigurationException($"Job {job.Name} missing \"Config\" item \"GameRoot\", required by plugin \"{this.ContextPluginConfig.Key}\".");
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

            // for example :     //mydepot/main/core/PSD/Game
            string gameRemoteRoot = job.Config.First(c => c.Key == "GameRoot").Value.ToString();

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
            string client = null;
            CachePayload clientLookup = cache.Get(this,job, build, hash);
            if (clientLookup.Payload != null)
                client = clientLookup.Payload;

            if (client == null) 
            {
                Match match = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Match(rawLog);
                if (match.Success)
                {
                    client = match.Groups[1].Value.Trim();
                    cache.Write(this, job, build, hash, client);
                }
            }

            Client p4client = null;
            if (client != null)
                p4client = PerforceUtils.ParseClient(client);

            string breakExtraFlag = string.Empty;
            bool isShaderError = false;
            bool isBluePrintError = false;
            bool isCPPError = false;
            List<string> blamedUserNames = new List<string>();
            string[] allowedParsers = { "Wbtb.Extensions.LogParsing.Unreal4LogParser", "Wbtb.Extensions.LogParsing.Cpp", "Wbtb.Extensions.LogParsing.BasicErrors" };
            bool p4CauseLinked = false;
            string fileCausingBreak = string.Empty;
            string specificErrorParsed = string.Empty;
            string basicErrorParsed = string.Empty;
            string parsedLogError = string.Empty;

            IList<string> implicatedRevisions = new List<string>();

            foreach (BuildLogParseResult buildLogParseResult in logParseResults)
            {
                ParsedBuildLogText parsedText = BuildLogTextParser.Parse(buildLogParseResult.ParsedContent);
                if (parsedText == null)
                    continue;

                foreach (ParsedBuildLogTextLine line in parsedText.Items)
                {
                    if (line.Items.Any(l => l.Type == "flag" && l.Content == "shader"))
                        isShaderError = true;

                    if (line.Items.Any(l => l.Type == "flag" && l.Content == "blueprint"))
                        isBluePrintError = true;

                    if (parsedText.Type == "Wbtb.Extensions.LogParsing.BasicErrors")
                        basicErrorParsed += $"{string.Join("\n", line.Items.Select(i => i.Content))}\n";
                    else
                        specificErrorParsed += $"{string.Join(" ", line.Items.Select(i => i.Content))} ";

                    foreach (ParsedBuildLogTextLineItem localPathItem in line.Items.Where(l => l.Type == "path")) 
                    {
                        if (!allowedParsers.Contains(parsedText.Type))
                            continue;

                        // force unitpaths, p4 uses these
                        string localFile = localPathItem.Content.Replace("\\", "/");

                        isCPPError = parsedText.Type == "Wbtb.Extensions.LogParsing.Cpp";

                        if (p4client == null)
                            continue;

                        // everything further requires p4client, so exit loop if none known
                        string clientRoot = p4client.Root.Replace("\\", "/");

                        foreach (Revision revision in revisionsLinkedToBuild)
                            foreach (string revisionFile in revision.Files)
                            {
                                // is revisionFile mentioned in a log error?
                                foreach (ClientView clientView in p4client.Views) 
                                {
                                    // currently support only standard mapping
                                    if (!clientView.Local.EndsWith("/..."))
                                        continue;

                                    p4CauseLinked = true;

                                    // clip off trailing /... from client mapping
                                    string root = clientView.Local.Substring(0, clientView.Local.Length - 4);

                                    // remove root of local path,  by replacing the known client root with empty string
                                    IList<string> pathfragments = localFile.Replace(clientRoot, string.Empty).Split("/", StringSplitOptions.RemoveEmptyEntries).ToList();

                                    // remove first item in path, this will always overlap with the p4 remap map, also ensure we have no padded or emptry items in path
                                    pathfragments = pathfragments.Select(r => r.Trim()).Where(r => r.Length > 0).Skip(1).ToList();

                                    string localFileMappedToRemote = string.Join("/", pathfragments);

                                    // add the remote map root to local path, so our local path should now be fully mapped to its remote equivalent
                                    localFileMappedToRemote = clientView.Remote.Substring(0, clientView.Remote.Length - 4) + "/" + localFileMappedToRemote;

                                    string revisionFileRemapped = revisionFile;

                                    if (isBluePrintError)
                                    {
                                        // blueprint errors always rooted in /<game>/Content, and instead of a file extension they list the blue print name, so need to remap that here
                                        // remove root of local path,  by replacing the known client root with empty string

                                        // remove evertything after last ".", this is the blue print name
                                        localFile = Regex.Replace(localFile, "[^.]*$", string.Empty);
                                        // remove trailing . too
                                        if (localFile.EndsWith("."))
                                            localFile = localFile.Substring(0, localFile.Length - 1);

                                        // clip off the first dir from path, this will be the game door dir, we don't want this
                                        localFile = string.Join("/", localFile.Split("/", StringSplitOptions.RemoveEmptyEntries).Skip(1));

                                        localFileMappedToRemote = localFile;
                                        
                                        string gameDoorDirectory = gameRemoteRoot.Split("/").Last();
                                        
                                        // if the revision file is in the game directory of project, remap it so it looks like a blue print file
                                        if (revisionFileRemapped.Contains(gameRemoteRoot + "/Content")) 
                                        {
                                            // remove bp extension + "." 
                                            revisionFileRemapped = Regex.Replace(revisionFileRemapped, "[^.]*$", string.Empty);
                                            if (revisionFileRemapped.EndsWith("."))
                                                revisionFileRemapped = revisionFileRemapped.Substring(0, revisionFileRemapped.Length - 1);

                                            revisionFileRemapped = revisionFileRemapped.Replace(gameRemoteRoot + "/Content/", string.Empty);
                                        }
                                    }

                                    if (localFileMappedToRemote != revisionFileRemapped)
                                        continue;

                                    BuildInvolvement bi = buildInvolvements.First(bi => bi.RevisionCode == revision.Code);
                                    bi.BlameScore = 100; // set score to max because we've directly tied user's revision file to build log error
                                    data.SaveBuildInvolement(bi);

                                    User blamedUser = data.GetUserById(bi.MappedUserId);
                                    if (blamedUser == null)
                                        blamedUserNames.Add(revision.User);
                                    else
                                        blamedUserNames.Add(blamedUser.Name);

                                    fileCausingBreak = revisionFile;
                                    implicatedRevisions.Add(revision.Code);
                                }
                            }
                    }
                }
            }
            
            string description = string.Empty;
            
            blamedUserNames = blamedUserNames.Distinct().ToList();
            implicatedRevisions = implicatedRevisions.Distinct().ToList();

            description = string.Empty;

            if (isBluePrintError)
                breakExtraFlag = " Blueprint error";

            if (isShaderError)
                breakExtraFlag = " Shader error";

            if (isCPPError)
                breakExtraFlag = " C++ error";

            if (string.IsNullOrEmpty(specificErrorParsed))
                description += $"Could not find definitive cause, 'error' keyword match returned:\n{basicErrorParsed}\n";
            else
                description += $"{specificErrorParsed}\n";

            if (!string.IsNullOrEmpty(fileCausingBreak))
                description += $"Caused by file {fileCausingBreak}.\n";

            if (implicatedRevisions.Any())
                description += $"Revision{(implicatedRevisions.Count == 1 ? "" : "s")} {string.Join(",", implicatedRevisions)}.\n";
            else
                description += $"Could not parse revision.\n";

            if (blamedUserNames.Any())
                description += $"Broken by {string.Join(",", blamedUserNames)}.\n";
            else
                description += $"Could not link error to user.\n";
            
            string summary = string.Empty;
            if (blamedUserNames.Any())
                summary = string.Join(",", blamedUserNames);

            if (summary.Length > 0)
                summary += $" ({breakExtraFlag})";
            else
                summary = breakExtraFlag;

            if (summary.Length == 0)
                summary = "Unknown error";

            data.SaveIncidentReport(new IncidentReport
            {
                IncidentId = build.IncidentBuildId,
                MutationId = build.Id,
                ImplicatedRevisions = implicatedRevisions,
                Processor = this.GetType().Name,
                Summary = summary,
                Status = "Break",
                Description = description
            });

            return new PostProcessResult
            {
                Passed = true,
                Result = summary
            };

            // if log parse results are blueprint
            // can we parse blueprint file path out of error message
            // foreach file in buildinvolvements revisions
            // can we connect revision file path to blueprint file path

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
            string processSummary = "No matching errors found";

            if (client == null)
                processSummary = $"Could not parse out p4 clientspec from log.";

            return new PostProcessResult
            {
                Passed = true,
                Result = processSummary
            };

        }
    }
}
