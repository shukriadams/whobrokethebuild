using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.BuildServer.Jenkins
{
    public class Jenkins : Plugin, IBuildServerPlugin
    {
        #region UTIL

        public PluginInitResult InitializePlugin()
        {

            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        public ReachAttemptResult AttemptReach(Core.Common.BuildServer contextServer)
        {
            if (!contextServer.Config.Any(c => c.Key == "Host"))
                throw new ConfigurationException("Missing item \"Host\"");

            if (!contextServer.Config.Any(c => c.Key == "Username"))
                throw new ConfigurationException("Missing item \"Username\"");

            if (!contextServer.Config.Any(c => c.Key == "Token"))
                throw new ConfigurationException("Missing item \"Token\"");

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

        public void AttemptReachJob(Core.Common.BuildServer buildServer, Job job)
        {
            WebClient webClient = this.GetAuthenticatedWebclient(buildServer);
            string hostUrl = GetHostUrl(buildServer);
            string url = UrlHelper.Join(hostUrl, "job", job.Key, "api/json?pretty=true");
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

        public void VerifyBuildServerConfig(Core.Common.BuildServer buildServer) 
        { 

        }

        public string GetBuildUrl(Core.Common.BuildServer contextServer, Build build)
        {
            if (string.IsNullOrEmpty(contextServer.Url))
                return null;

            IDataLayerPlugin datalayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            Job job = datalayer.GetJobById(build.JobId);
            return new Uri(new Uri(contextServer.Url), $"job/{job.Key}/{build.Identifier}").ToString();
        }

        public string GetEphemeralBuildLog(Build build)
        {
            try
            {
                IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
                Job job = dataLayer.GetJobById(build.JobId);
                Core.Common.BuildServer buildServer = dataLayer.GetBuildServerById(job.BuildServerId);
                WebClient webClient = this.GetAuthenticatedWebclient(buildServer);

                string hostUrl = GetHostUrl(buildServer);
                string url = UrlHelper.Join(hostUrl, "job", job.Key, build.Identifier, "consoleText");
                string log = webClient.DownloadString(url);
                return log;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download log for build {build.Id}, external id {build.Identifier}", ex);
            }
        }

        private string GetAndStoreBuildLog(Build build)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            // persist path
            Job job = dataLayer.GetJobById(build.JobId);

            string persistPath = Path.Combine(ConfigKeeper.Instance.PluginDataPersistDirectory, this.ContextPluginConfig.Key, job.Key, build.Identifier, "log.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
            if (File.Exists(persistPath))
                return File.ReadAllText(persistPath);
            
            try
            {
                string log = this.GetEphemeralBuildLog(build);
                File.WriteAllText(persistPath, log);
                return log;
            }
            catch(Exception ex)
            { 
                throw new Exception($"Failed to download log for build {build.Id}, external id {build.Identifier}", ex);
            }
        }

        private WebClient GetAuthenticatedWebclient(Core.Common.BuildServer buildServer)
        {
            WebClient webClient = new WebClient();

            string username = buildServer.Config.First(r => r.Key == "Username").Value.ToString();
            string token = buildServer.Config.First(r => r.Key == "Token").Value.ToString();

            WebHeaderCollection headers = new WebHeaderCollection();
            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{username}:{token}"));

            headers.Add(HttpRequestHeader.Authorization, $"Basic {svcCredentials}");
            webClient.Headers = headers;
            return webClient;
        }

        public IEnumerable<string> ListRemoteJobsCanonical(Core.Common.BuildServer buildServer)
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

        public IEnumerable<User> GetUsersInBuild(Core.Common.BuildServer contextServer, string remoteBuildId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Queries Jenkins to get confirmed revisions in build - this works only on builds where revision changes trigger builds, it will return nothing
        /// for builds that run on a fixed timer.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="build"></param>
        /// <returns></returns>
        private IEnumerable<string> GetRevisionsInBuild(Job job, Build build)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            string persistPath = PersistPathHelper.GetPath(this, job.Key, build.Identifier, "revisions.json");

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
                string url = UrlHelper.Join(hostUrl, "job", job.Key, build.Identifier, "api/json?pretty=true&tree=changeSet[items[commitId]]");
                rawJson = webClient.DownloadString(url);

                Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
                File.WriteAllText(persistPath, rawJson);

            }

            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJson);
            IEnumerable<RawRevision> rawRevisions = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RawRevision>>(response.changeSet.items.ToString());
            return rawRevisions.Select(r => r.commitId); 
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
                    return BuildStatus.Failed;

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

        public BuildImportSummary ImportAllCachedBuilds(Job job) 
        {
            IList<RawBuild> rawBuilds = new List<RawBuild>();

            foreach (string directory in Directory.GetDirectories(PersistPathHelper.GetPath(this, job.Key)).Where(d => !string.IsNullOrEmpty(d)))
            {
                string rawBuildFile = Path.Combine(directory, "raw.json");
                if (File.Exists(rawBuildFile))
                    rawBuilds.Add(JsonConvert.DeserializeObject<RawBuild>(File.ReadAllText(rawBuildFile)));
            }

            return this.ImportBuildsInternal(job, rawBuilds);
        }

        public BuildImportSummary ImportBuilds(Job job, int take)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            Core.Common.BuildServer buildServer = dataLayer.GetBuildServerById(job.BuildServerId);

            WebClient webClient = this.GetAuthenticatedWebclient(buildServer);
            string hostUrl = GetHostUrl(buildServer);
            string url = UrlHelper.Join(hostUrl, "job", job.Key, "api/json?pretty=true&tree=allBuilds[fullDisplayName,id,number,timestamp,duration,builtOn,result]");
            string rawJson = webClient.DownloadString(url);

            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJson);
            
            IEnumerable<RawBuild> rawBuilds = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RawBuild>>(response.allBuilds.ToString());
            if (take != 0)
                rawBuilds = rawBuilds
                    .Take(take);

            return this.ImportBuildsInternal(job, rawBuilds);
        }

        private BuildImportSummary ImportBuildsInternal(Job job, IEnumerable<RawBuild> rawBuilds)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            BuildImportSummary summary = new BuildImportSummary();

            foreach (RawBuild rawBuild in rawBuilds)
            {
                Build build = dataLayer.GetBuildByKey(job.Id, rawBuild.number);

                if (build == null)
                {
                    // 
                    build = new Build
                    {
                        Identifier = rawBuild.number,
                        JobId = job.Id,
                        Hostname = rawBuild.builtOn,
                        StartedUtc = UnixTimeStampToDateTime(rawBuild.timestamp)
                    };

                    summary.Created.Add(build);
                    Console.WriteLine($"Imported build {build.Identifier}");
                }

                build.Status = ConvertBuildStatus(rawBuild.result);

                if (build.EndedUtc == null &&!string.IsNullOrEmpty(rawBuild.duration)){
                    build.EndedUtc = build.StartedUtc.AddMilliseconds(int.Parse(rawBuild.duration));
                    summary.Ended.Add(build);
                }

                build = dataLayer.SaveBuild(build);

                IEnumerable<string> buildRevisions = GetRevisionsInBuild(job, build);

                foreach (string buildRevision in buildRevisions)
                {
                    BuildInvolvement buildInvolvement = dataLayer.GetBuildInvolvementByRevisionCode(job.Id, buildRevision);
                    if (buildInvolvement == null)
                    {
                        buildInvolvement = new BuildInvolvement
                        {
                            BuildId = build.Id,
                            RevisionCode = buildRevision
                        };

                        dataLayer.SaveBuildInvolement(buildInvolvement);

                        Console.WriteLine($"Added build involvement revision \"{buildInvolvement.RevisionCode}\" to build \"{build.Identifier}\".");
                    }
                }
            }

            return summary;
        }

        public IEnumerable<Build> ImportLogs(Job job)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            IEnumerable<Build> buildsWithNoLog = dataLayer.GetBuildsWithNoLog(job);
            string logPath = string.Empty;
            IList<Build> processedBuilds = new List<Build>(); // not used - remove this

            foreach (Build buildWithNoLog in buildsWithNoLog)
            {
                try 
                {
                    string logContent = GetAndStoreBuildLog(buildWithNoLog);
                    string logDirectory = Path.Combine(ConfigKeeper.Instance.BuildLogsDirectory, GetRandom(), GetRandom());

                    Directory.CreateDirectory(logDirectory);
                    logPath = Path.Combine(logDirectory, $"{Guid.NewGuid()}.txt");
                    File.WriteAllText(logPath, logContent);

                    buildWithNoLog.LogPath = logPath;
                    dataLayer.SaveBuild(buildWithNoLog);

                    processedBuilds.Add(buildWithNoLog);
                    // if job relies on log-based revision linking, execute that now
                    Console.WriteLine($"Imported log for build {buildWithNoLog.Id}");
                }
                catch(Exception ex)
                { 
                    try 
                    {
                        if (File.Exists(logPath))
                            File.Delete(logPath);
                    }
                    catch(Exception exCleanup)
                    { 
                        Console.WriteLine($"Unexpected error trying to rollback log @ {logPath}", exCleanup);
                    }

                    // yeah, what is going on here .....
                    // ignore network errors
                    Console.WriteLine($"Error fetching log for build {buildWithNoLog.Id}", ex);
                }
            }

            return processedBuilds;
        }

        #endregion
    }
}
