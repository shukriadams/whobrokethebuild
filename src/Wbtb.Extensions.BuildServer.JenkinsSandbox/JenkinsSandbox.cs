using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.BuildServer.JenkinsSandbox
{
    public class JenkinsSandbox : Plugin, IBuildServerPlugin
    {
        #region FIELDS

        private readonly Config _config;

        private readonly PluginProvider _pluginProvider;

        private readonly PersistPathHelper _persistPathHelper;

        private readonly SimpleDI _di;
        #endregion

        #region CTORS

        public JenkinsSandbox(Config config, PluginProvider pluginProvider, PersistPathHelper persistPathHelper) 
        { 
            _config = config;
            _pluginProvider = pluginProvider;
            _persistPathHelper = persistPathHelper;
            _di = new SimpleDI();
        }

        #endregion

        #region UTIL

        public PluginInitResult InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        public void Diagnose() 
        {
            IDataLayerPlugin data = _di.Resolve<IDataLayerPlugin>();
            var records = data.GetBuildServers();
        }

        public ReachAttemptResult AttemptReach(Core.Common.BuildServer contextServer)
        {
            // sandbox should by default always be reachable. Change at dev time to simulate failures.
            return new ReachAttemptResult { Reachable = true };
        }

        public void AttemptReachJob(Core.Common.BuildServer buildServer, Job job)
        {
            // do nothing, assume always passes
        }

        #endregion

        #region METHODS

        public void VerifyBuildServerConfig(Core.Common.BuildServer buildServer)
        {

        }


        private static PluginConfig GetThisConfig()
        {
            return null;
        }

        public string GetBuildUrl(Core.Common.BuildServer contextServer, Build build)
        {
            if (string.IsNullOrEmpty(contextServer.Url))
                return null;

            IDataLayerPlugin datalayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            Job job = datalayer.GetJobById(build.JobId);
            return new Uri(new Uri(contextServer.Url), $"job/{job.Key}/{build.Identifier}").ToString();
        }


        public IEnumerable<string> ListRemoteJobsCanonical(Core.Common.BuildServer buildServer)
        {
            string filePath = "./JSON/jobs.json";
            string rawJson = ResourceHelper.ReadResourceAsString(this.GetType(), "JSON.jobs.json");

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

        private IEnumerable<string> GetRevisionsInBuild(Job job, Build build)
        {
            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            string filePath = $"./JSON/builds/{job.Key}/build_{build.Identifier}_revisions.json";
            string rawJson = null;

            string persistPath = _persistPathHelper.GetPath(this, job.Key, build.Identifier, "revisions.json");
            
            if (File.Exists(persistPath)) 
            {
                rawJson = File.ReadAllText(persistPath);
            }
            else 
            {
                string path = $"JSON.builds.{job.Key}.build_{build.Identifier}_revisions.json";
                if (ResourceHelper.ResourceExists(this.GetType(), path)) 
                { 
                    rawJson = ResourceHelper.ReadResourceAsString(this.GetType(), path);
                    Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
                    File.WriteAllText(persistPath, rawJson);
                }
                else
                    Console.WriteLine($"Failed to to read revisions for build {build.Identifier}, job {job.Name}. No data in sandbox");
            }
            
            if (rawJson == null)
                return new string[] { };

            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJson);
            IEnumerable<RawRevision> rawRevisions = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RawRevision>>(response.changeSet.items.ToString());
            return rawRevisions.Select(r => r.commitId);
        }

        public BuildImportSummary ImportBuilds(Job job, int take)
        {
            string resourcePath = $"JSON.builds.{job.Key}.builds.json";
            
            if (!ResourceHelper.ResourceExists(this.GetType(), resourcePath))
            {
                Console.WriteLine($"Failed to import builds, sandbox job {job.Key} does not exist in data store");
                return new BuildImportSummary();
            }
                    
            string rawJson = ResourceHelper.ReadResourceAsString(this.GetType(), resourcePath);
            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJson);
            IEnumerable<RawBuild> rawBuilds = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RawBuild>>(response.allBuilds.ToString());
            
            // persist each rawbuild if necessary
            foreach(RawBuild rawbuild in rawBuilds) 
            {
                string persistPath = _persistPathHelper.GetPath(this, job.Key, rawbuild.number, "raw.json");
                if (!File.Exists(persistPath)) 
                { 
                    Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
                    File.WriteAllText(persistPath, JsonConvert.SerializeObject(rawbuild));
                }
            }

            if (take != 0)
                rawBuilds = rawBuilds
                    .Take(take);

            return this.ImportBuildsInternal(job, rawBuilds);
        }

        public BuildImportSummary ImportAllCachedBuilds(Job job)
        { 
            IList<RawBuild> rawBuilds = new List<RawBuild>();
            string cachePath = _persistPathHelper.GetPath(this, job.Key);

            if (Directory.Exists(cachePath))
                foreach(string directory in Directory.GetDirectories(cachePath).Where(d => !string.IsNullOrEmpty(d)))
                { 
                    string rawBuildFile = Path.Combine(directory, "raw.json");
                    if (File.Exists(rawBuildFile))
                    {
                        string rawJson = File.ReadAllText(rawBuildFile);
                        rawBuilds.Add(JsonConvert.DeserializeObject<RawBuild>(rawJson));
                    }
                }

            return this.ImportBuildsInternal(job, rawBuilds);
        }

        private BuildImportSummary ImportBuildsInternal(Job job, IEnumerable<RawBuild> rawBuilds)
        {
            BuildImportSummary summary = new BuildImportSummary();
            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

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

                    Console.WriteLine($"Imported build {build.Identifier}");
                }

                bool isChanged = false;
                build.Status = ConvertBuildStatus(rawBuild.result);
                if (!string.IsNullOrEmpty(rawBuild.duration) && !build.EndedUtc.HasValue){
                    build.EndedUtc = build.StartedUtc.AddMilliseconds(int.Parse(rawBuild.duration));
                    isChanged  = true;
                }

                bool isCreating = build.Id == null;
                if (isCreating || isChanged)
                    build = dataLayer.SaveBuild(build);
                
                if (isCreating)
                    summary.Created.Add(build);
            
                if (isChanged)
                    summary.Ended.Add(build);

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

        private static string GetRandom()
        {
            string chars = "abcdefghijklmnopqrstuvwxyz1234567890";
            Random random = new Random();
            int position = random.Next(0, chars.Length);
            return chars.Substring(position, 1);
        }

        public string GetEphemeralBuildLog(Build build)
        {
            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            // persist path
            Job job = dataLayer.GetJobById(build.JobId);

            string log = ResourceHelper.ReadResourceAsString(this.GetType(), $"JSON.builds.{job.Key}.logs.{build.Identifier}.txt");
            return log;
        }

        private string GetBuildLog(Build build)
        {
            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            // persist path
            Job job = dataLayer.GetJobById(build.JobId);

            string persistPath = _persistPathHelper.GetPath(this, job.Key, build.Identifier, "log.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
            if (File.Exists(persistPath))
                return File.ReadAllText(persistPath);

            try
            {
                string log = GetEphemeralBuildLog(build);
                File.WriteAllText(persistPath, log);
                return log;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download log for build {build.Id}, external id {build.Identifier}", ex);
            }
        }

        public IEnumerable<Build> ImportLogs(Job job)
        {
            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            IEnumerable<Build> buildsWithNoLog = dataLayer.GetBuildsWithNoLog(job);
            string logPath = string.Empty;
            IList<Build> processedBuilds = new List<Build>(); // not used - remove this

            foreach (Build buildWithNoLog in buildsWithNoLog)
            {
                try
                {
                    if (!ResourceHelper.ResourceExists(this.GetType(), $"JSON.builds.{job.Key}.logs.{buildWithNoLog.Identifier}.txt"))
                            continue;

                    string logContent = GetBuildLog(buildWithNoLog);
                    string logDirectory = Path.Combine(_config.BuildLogsDirectory, GetRandom(), GetRandom());

                    Directory.CreateDirectory(logDirectory);
                    logPath = Path.Combine(logDirectory, $"{Guid.NewGuid()}.txt");
                    File.WriteAllText(logPath, logContent);

                    buildWithNoLog.LogPath = logPath;
                    dataLayer.SaveBuild(buildWithNoLog);

                    processedBuilds.Add(buildWithNoLog);
                    Console.WriteLine($"Imported log for build {buildWithNoLog.Id}");
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (File.Exists(logPath))
                            File.Delete(logPath);
                    }
                    catch (Exception exCleanup)
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
