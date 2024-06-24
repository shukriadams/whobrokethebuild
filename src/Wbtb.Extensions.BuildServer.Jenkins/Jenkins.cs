using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.BuildServer.Jenkins
{
    public class Jenkins : Plugin, IBuildServerPlugin
    {
        #region FIELDS

        private readonly Configuration _config;

        private readonly PluginProvider _pluginProvider;

        private readonly PersistPathHelper _persistPathHelper;

        private readonly SimpleDI _di;

        #endregion

        #region CTORS

        public Jenkins(Configuration config, PluginProvider pluginProvider, PersistPathHelper persistPathHelper) 
        {
            _config = config;
            _pluginProvider = pluginProvider;
            _persistPathHelper = persistPathHelper;
            _di = new SimpleDI();
        }

        #endregion

        #region UTIL

        PluginInitResult IPlugin.InitializePlugin()
        {

            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

         ReachAttemptResult IBuildServerPlugin.AttemptReach(Core.Common.BuildServer contextServer)
        {
            if (contextServer.Config == null)
                throw new ConfigurationException("Missing item \"Config\"");

            if (!contextServer.Config.Any(c => c.Key == "Host"))
                throw new ConfigurationException("Missing Config item \"Host\"");

            if (!contextServer.Config.Any(c => c.Key == "Username"))
                throw new ConfigurationException("Missing Config item \"Username\"");

            if (!contextServer.Config.Any(c => c.Key == "Token"))
                throw new ConfigurationException("Missing Config item \"Token\"");

            Configuration config = _di.Resolve<Configuration>();

            string persistDirectory = _persistPathHelper.EnsurePluginPersistDirectory(ContextPluginConfig);

            // each job should have RemoteKey config item
            foreach (Job job in contextServer.Jobs) 
            {
                if (job.Config == null)
                    throw new ConfigurationException($"Job {job.Key} on buildServer {contextServer.Key} is missing Config node");

                if (!job.Config.Any(c => c.Key == "RemoteKey"))
                    throw new ConfigurationException($"Job {job.Key} on buildServer {contextServer.Key} is missing Config \"RemoteKey\"");

                
                string jobPersistPath = Path.Combine(persistDirectory, job.Key);
                Directory.CreateDirectory(jobPersistPath);
            }

            try
            {
                WebClient webClient = GetAuthenticatedWebclient(contextServer);
                string host = contextServer.Config.First(r => r.Key == "Host").Value.ToString();
                string url = UrlHelper.Join(host, "api/json?pretty=true&tree=jobs[name]"); // do some arb thing
                webClient.DownloadString(url);

                return new ReachAttemptResult{Reachable = true };
            }
            catch (Exception ex)
            { 
                return new ReachAttemptResult{ Exception = ex };
            }
        }

        void IBuildServerPlugin.AttemptReachJob(Core.Common.BuildServer buildServer, Job job)
        {
            var remotekey = job.Config.FirstOrDefault(c => c.Key == "RemoteKey");

            WebClient webClient = this.GetAuthenticatedWebclient(buildServer);
            string hostUrl = GetHostUrl(buildServer);
            string url = UrlHelper.Join(hostUrl, "job", remotekey.Value.ToString(), "api/json?pretty=true");
            webClient.DownloadString(url);
        }

        #endregion

        #region METHODS

        private static string GetHostUrl(Core.Common.BuildServer buildServer)
        {
            string host = buildServer.Config.First(r => r.Key == "Host").Value.ToString();

            if (!host.ToLower().StartsWith("http") && !host.ToLower().StartsWith("https"))
                host = $"http://{host}";

            return host;
        }

        void IBuildServerPlugin.VerifyBuildServerConfig(Core.Common.BuildServer buildServer) 
        { 

        }

        string IBuildServerPlugin.GetBuildUrl(Core.Common.BuildServer contextServer, Build build)
        {
            string host = contextServer.Config.First(r => r.Key == "Host").Value.ToString();
            IDataPlugin datalayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = datalayer.GetJobById(build.JobId);
            KeyValuePair<string, object> remoteKey = job.Config.FirstOrDefault(r => r.Key == "RemoteKey");

            return new Uri(new Uri(host), $"job/{remoteKey.Value}/{build.Key}").ToString();
        }

        string IBuildServerPlugin.GetEphemeralBuildLog(Build build)
        {
            // do not try/catch this, caller should handle errors directly
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            Core.Common.BuildServer buildServer = dataLayer.GetBuildServerById(job.BuildServerId);
            KeyValuePair<string, object> remoteKey = job.Config.FirstOrDefault(r => r.Key == "RemoteKey");

            WebClient webClient = this.GetAuthenticatedWebclient(buildServer);

            string hostUrl = GetHostUrl(buildServer);
            string url = UrlHelper.Join(hostUrl, "job", remoteKey.Value.ToString(), build.Key, "consoleText");
            string log = webClient.DownloadString(url);
            return log;
        }

        private string GetAndStoreBuildLog(Build build)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            // persist path
            Job job = dataLayer.GetJobById(build.JobId);

            string persistPath = Path.Combine(_config.PluginDataPersistDirectory, this.ContextPluginConfig.Manifest.Key, job.Key, build.Key, "log.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
            if (File.Exists(persistPath))
                return File.ReadAllText(persistPath);
            
            try
            {
                string log = ((IBuildServerPlugin)this).GetEphemeralBuildLog(build);
                File.WriteAllText(persistPath, log);
                return log;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    HttpWebResponse resp = (HttpWebResponse)ex.Response;
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        string placeholdertext = "Log no longer available on Jenkins.";
                        File.WriteAllText(persistPath, placeholdertext);
                        ConsoleHelper.WriteLine($"Jenkins no longer has revision listing for build {build.Id}, forcing empty");
                        return placeholdertext;
                    }
                    else
                    {
                        throw ex;
                    }
                }
                else
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            { 
                throw new Exception($"Failed to download log for build {build.Id}, external id {build.Key}", ex);
            }
        }

        private WebClient GetAuthenticatedWebclient(Core.Common.BuildServer buildServer)
        {
            WebClient client = new WebClient();

            string username = buildServer.Config.First(r => r.Key == "Username").Value.ToString();
            string token = buildServer.Config.First(r => r.Key == "Token").Value.ToString();

            WebHeaderCollection headers = new WebHeaderCollection();
            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{username}:{token}"));

            headers.Add(HttpRequestHeader.Authorization, $"Basic {svcCredentials}");
            client.Headers = headers;
            return client;
        }

        IEnumerable<string> IBuildServerPlugin.ListRemoteJobsCanonical(Core.Common.BuildServer buildServer)
        {
            WebClient webClient = null;
            string rawJson = string.Empty;

            webClient = GetAuthenticatedWebclient(buildServer);
            string host = GetHostUrl(buildServer);
            string url = UrlHelper.Join(host, "api/json?pretty=true&tree=jobs[name]");

            try
            {
                rawJson = webClient.DownloadString(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error contacting host {host}", ex);
            }

            try
            {
                dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJson);
                IEnumerable<RawJob> jobs = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RawJob>>(response.jobs.ToString());
                return jobs.Select(r => r.name);
            }
            catch (Exception ex)
            { 
                throw new Exception($"Failed to parse JSON content {rawJson}", ex);
            }
        }


        /// <summary>
        /// Queries Jenkins to get confirmed revisions in build - this works only on builds where revision changes trigger builds, it will return nothing
        /// for builds that run on a fixed timer.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="build"></param>
        /// <returns></returns>
        BuildRevisionsRetrieveResult IBuildServerPlugin.GetRevisionsInBuild(Build build)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            string persistPath = _persistPathHelper.GetPath(this.ContextPluginConfig, job.Key, "revisions", $"{build.Key}.json");

            Core.Common.BuildServer buildServer = dataLayer.GetBuildServerByKey(job.BuildServer);
            string rawJson;

            if (File.Exists(persistPath))
            {
                rawJson = File.ReadAllText(persistPath);
            }
            else
            {
                WebClient webClient = this.GetAuthenticatedWebclient(buildServer);
                string hostUrl = GetHostUrl(buildServer);
                var remoteKey = job.Config.FirstOrDefault(r => r.Key == "RemoteKey");
                string url = UrlHelper.Join(hostUrl, "job", remoteKey.Value.ToString(), build.Key, "api/json?pretty=true&tree=changeSet[items[commitId]]");
                
                try 
                {
                    rawJson = webClient.DownloadString(url);
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                    {
                        HttpWebResponse resp = (HttpWebResponse)ex.Response;
                        if (resp.StatusCode != HttpStatusCode.NotFound)
                            throw ex;

                        ConsoleHelper.WriteLine($"Jenkins no longer has revision listing for build {build.Id}, forcing empty");
                        return new BuildRevisionsRetrieveResult { Result = "Revisions no longer available on server <WBTB_BUILDSERVER_HISTORY_EXPIRED>.", Success = true };
                    }
                    else
                    {
                        throw ex;
                    }
                }

                Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
                File.WriteAllText(persistPath, rawJson);
            }

            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJson);
            IEnumerable<RawRevision> rawRevisions = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RawRevision>>(response.changeSet.items.ToString());
            return new BuildRevisionsRetrieveResult { Revisions = rawRevisions.Select(r => r.commitId), Success = true }; 
        }

        private BuildStatus ConvertBuildStatus(string remoteResult)
        {
            switch (remoteResult)
            {
                case "FAILURE":
                    return BuildStatus.Failed;

                case "SUCCESS":
                    return BuildStatus.Passed;

                case "ABORTED":
                    return BuildStatus.Aborted;

                case null:
                    return BuildStatus.InProgress;

                default:
                    return BuildStatus.Unknown;
            }
        }

        private static DateTime UnixTimeStampToDateTime(string unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            return dt.AddMilliseconds(Double.Parse(unixTimeStamp));
        }

        private static string GetRandom()
        {
            string chars = "abcdefghijklmnopqrstuvwxyz1234567890";
            Random random = new Random();
            int position = random.Next(0, chars.Length);
            return chars.Substring(position, 1);
        }

        void IBuildServerPlugin.PollBuildsForJob(Job job) 
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Core.Common.BuildServer buildServer = dataLayer.GetBuildServerById(job.BuildServerId);
            var remoteKey = job.Config.First(r => r.Key == "RemoteKey");

            WebClient webClient = this.GetAuthenticatedWebclient(buildServer);

            string hostUrl = GetHostUrl(buildServer);
            string url = UrlHelper.Join(hostUrl, "job", remoteKey.Value.ToString(), "api/json?pretty=true&tree=allBuilds[fullDisplayName,id,number,timestamp,duration,builtOn,result]");
            string rawJson = webClient.DownloadString(url);
            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJson);

            IEnumerable<RawBuild> remoteBuilds = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RawBuild>>(response.allBuilds.ToString());
            foreach (RawBuild remoteBuild in remoteBuilds) 
            {
                string persistPath = string.Empty;
                string cleanupPath = string.Empty;

                // if remote build is still in progress (no result) we write it to "incomplete" builds.
                // Else we write to builds root directory
                if (remoteBuild.result == null)
                {
                    persistPath = _persistPathHelper.GetPath(this.ContextPluginConfig, job.Key, "incomplete", $"{remoteBuild.number}", "build.json");
                }
                else
                {
                    persistPath = _persistPathHelper.GetPath(this.ContextPluginConfig, job.Key, $"{remoteBuild.number}", "build.json");

                    // need to clean out the in-progress builds
                    cleanupPath = _persistPathHelper.GetPath(this.ContextPluginConfig, job.Key, "incomplete", $"{remoteBuild.number}", "build.json");
                }

                if (!File.Exists(persistPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
                    File.WriteAllText(persistPath, JsonConvert.SerializeObject(remoteBuild));
                }

                if (!string.IsNullOrEmpty(cleanupPath) && File.Exists(cleanupPath))
                    File.Delete(cleanupPath);
            }
        }

        private RawBuild LoadRawBuild(string path) 
        {
            string fileContent;
            try
            {
                fileContent = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not read content of file {path}", ex);
            }

            RawBuild rawBuild;

            try
            {
                rawBuild = Newtonsoft.Json.JsonConvert.DeserializeObject<RawBuild>(fileContent);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse contents of file {path} to {typeof(RawBuild).Name}.", ex);
            }

            return rawBuild;
        }

        /// <summary>
        /// This method does not directly call server to get latest builds, the PollBuildsForJob method does that. That method also writes everything
        /// it finds in the "incomplete" directory. 
        /// 
        /// This method reads all builds under "incomplete" and complete, and returns the latest of all them combined, so it overreads (safety). The 
        /// daemon calling this will discard overreads after verifying database state.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<Build> IBuildServerPlugin.GetLatesBuilds(Job job, int take)
        {
            string incompleteLookupPath = _persistPathHelper.GetPath(this.ContextPluginConfig, job.Key, "incomplete");
            string completeLookupPath = _persistPathHelper.GetPath(this.ContextPluginConfig, job.Key);

            Directory.CreateDirectory(incompleteLookupPath);

            // start by getting everything in incomplete, we will return all of these
            IList<string> buildsToReturn = Directory.GetFiles(incompleteLookupPath).ToList();

            // take a subset of builds in complete, this list will get large, so we want the latest ones. Jenkins uses numerical
            // build numbers so we can sort descending, but build numbers are variable-length strings, so a custom sorter is used
            IEnumerable<string> completeBuildParents = Directory.GetDirectories(completeLookupPath);
            
            List<string>sortedCompletedFilenames = completeBuildParents.Select(f => Path.GetFileName(f)).ToList();
            sortedCompletedFilenames.Sort(new NumericStringComparer());
            sortedCompletedFilenames = sortedCompletedFilenames
                .Reverse<string>() // compare sorts asc, we need desc
                .Take(take)
                .ToList();

            foreach (string completeBuildParent in completeBuildParents)
            {
                if (!sortedCompletedFilenames.Contains(Path.GetFileName(completeBuildParent)))
                    continue;

                string filePath = Path.Combine(completeBuildParent, "build.json");
                if (File.Exists(filePath))
                    buildsToReturn.Add(filePath);
            }

            IList<Build> builds = new List<Build>();
            foreach (string incompleteBuildFilePath in buildsToReturn) 
            {
                RawBuild rawBuild = this.LoadRawBuild(incompleteBuildFilePath);

                builds.Add(new Build
                {
                    Key = rawBuild.number,
                    Hostname = rawBuild.builtOn,
                    StartedUtc = UnixTimeStampToDateTime(rawBuild.timestamp),
                    Status = ConvertBuildStatus(rawBuild.result)
                });
            }

            return builds;
        }

        Build IBuildServerPlugin.TryUpdateBuild(Build build) 
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            string completeBuildPath = _persistPathHelper.GetPath(this.ContextPluginConfig, job.Key, $"{build.Key}", "build.json");
            if (!File.Exists(completeBuildPath))
                return build;

            RawBuild rawBuild = this.LoadRawBuild(completeBuildPath);
            if (rawBuild.result == null)
                return build;

            build.EndedUtc = build.StartedUtc.AddMilliseconds(int.Parse(rawBuild.duration));
            build.Status = ConvertBuildStatus(rawBuild.result);
            return build;
        }

        IEnumerable<Build> IBuildServerPlugin.GetAllCachedBuilds(Job job)
        {
            IList<string> buildDirectories = new List<string>();
            if (Directory.Exists(_persistPathHelper.GetPath(this.ContextPluginConfig, job.Key)))
                buildDirectories = buildDirectories.Concat(Directory.GetDirectories(_persistPathHelper.GetPath(this.ContextPluginConfig, job.Key))).ToList();

            IList<Build> builds = new List<Build>();

            foreach (string directory in buildDirectories)
            {
                string buildFile = Path.Combine(directory, "build.json");
                if (File.Exists(buildFile))
                {
                    RawBuild rawBuild = JsonConvert.DeserializeObject<RawBuild>(File.ReadAllText(buildFile));
                    DateTime started = UnixTimeStampToDateTime(rawBuild.timestamp);
                    builds.Add(new Build {
                        Key = rawBuild.number,
                        Hostname = rawBuild.builtOn,
                        StartedUtc = started,
                        EndedUtc = rawBuild.result == null ? null : started.AddMilliseconds(int.Parse(rawBuild.duration)),
                        Status = ConvertBuildStatus(rawBuild.result)
                    });
                }
            }

            return builds.OrderByDescending(b => b.StartedUtc);
        }

        BuildLogRetrieveResult IBuildServerPlugin.ImportLog(Build build)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            string absoluteLogPath = Build.GetLogPath(_config, job, build);

            try
            {

                if (File.Exists(absoluteLogPath))
                    return new BuildLogRetrieveResult { Success = true, Result = "Log already imported" };

                Directory.CreateDirectory(Path.GetDirectoryName(absoluteLogPath));

                string logContent = ((IBuildServerPlugin)this).GetEphemeralBuildLog(build);
                File.WriteAllText(absoluteLogPath, logContent);

                return new BuildLogRetrieveResult { Success = true };
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    HttpWebResponse resp = (HttpWebResponse)ex.Response;
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        string placeholdertext = "Log no longer available on server <WBTB_BUILDSERVER_HISTORY_EXPIRED>.";
                        File.WriteAllText(absoluteLogPath, placeholdertext);
                        ConsoleHelper.WriteLine($"Jenkins no longer has revision listing for build {build.Id}, forcing empty");
                        return new BuildLogRetrieveResult { Success = true, Result = placeholdertext };
                    }
                    else
                    {
                        throw ex;
                    }
                }
                else
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            { 
                try 
                {
                    if (File.Exists(absoluteLogPath))
                        File.Delete(absoluteLogPath);
                }
                catch(Exception exCleanup)
                {
                    ConsoleHelper.WriteLine($"Unexpected error trying to rollback log @ {absoluteLogPath}", exCleanup);
                }

                // yeah, what is going on here .....
                // ignore network errors
                ConsoleHelper.WriteLine($"Error fetching log for build {build.Id}", ex);
                return new BuildLogRetrieveResult { Result = $"Error fetching log for build {build.Id}, {ex}" };
            }

        }

        #endregion
    }
}
