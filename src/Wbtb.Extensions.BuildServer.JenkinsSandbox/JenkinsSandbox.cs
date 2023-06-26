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

        private readonly Configuration _config;

        private readonly PluginProvider _pluginProvider;

        private readonly PersistPathHelper _persistPathHelper;

        private readonly SimpleDI _di;
        #endregion

        #region CTORS

        public JenkinsSandbox(Configuration config, PluginProvider pluginProvider, PersistPathHelper persistPathHelper) 
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
            IDataPlugin data = _di.Resolve<IDataPlugin>();
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
            return string.Empty;
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

        public IEnumerable<string> GetRevisionsInBuild(Build build)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            string filePath = $"./JSON/builds/{job.Key}/build_{build.Identifier}_revisions.json";
            string rawJson = null;
            string persistPath = _persistPathHelper.GetPath(ContextPluginConfig, job.Key, build.Identifier, "revisions.json");
            
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

        public IEnumerable<Build> GetLatesBuilds(Job job, int take)
        {
            string resourcePath = $"JSON.builds.{job.Key}.builds.json";

            if (!ResourceHelper.ResourceExists(this.GetType(), resourcePath))
                throw new Exception($"Failed to import builds, sandbox job {job.Key} does not exist in data store");

            string rawJson = ResourceHelper.ReadResourceAsString(this.GetType(), resourcePath);
            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJson);
            IEnumerable<RawBuild> rawBuilds = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RawBuild>>(response.allBuilds.ToString());

            IList<Build> builds = new List<Build>();
            foreach (RawBuild rawBuild in rawBuilds)
                builds.Add(new Build
                {
                    Identifier = rawBuild.number,
                    Hostname = rawBuild.builtOn,
                    StartedUtc = UnixTimeStampToDateTime(rawBuild.timestamp),
                    Status = ConvertBuildStatus(rawBuild.result)
                });

            return builds;
        }

        private RawBuild LoadRawBuild(string path)
        {
            string fileContent;
            if (!File.Exists(path))
                throw new Exception($"expected raw build file {path} not found.");

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

        public IEnumerable<Build> GetAllCachedBuilds(Job job)
        {
            IList<Build> builds = new List<Build>();
            string lookupPath = _persistPathHelper.GetPath(this.ContextPluginConfig, job.Key, "complete");
            if (!Directory.Exists(lookupPath))
                return new Build[] { };

            IEnumerable<string> completeBuildFiles = Directory.GetFiles(lookupPath);

            foreach (string completeBuildFile in completeBuildFiles)
            {
                RawBuild rawBuild = this.LoadRawBuild(completeBuildFile);
                DateTime started = UnixTimeStampToDateTime(rawBuild.timestamp);
                builds.Add(new Build()
                {
                    Identifier = rawBuild.number,
                    Hostname = rawBuild.builtOn,
                    StartedUtc = started,
                    Status = ConvertBuildStatus(rawBuild.result),
                    EndedUtc = started.AddMilliseconds(int.Parse(rawBuild.duration))
                });
            }

            return builds;
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
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            // persist path
            Job job = dataLayer.GetJobById(build.JobId);

            string log = ResourceHelper.ReadResourceAsString(this.GetType(), $"JSON.builds.{job.Key}.logs.{build.Identifier}.txt");
            return log;
        }

        private string GetBuildLog(Build build)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            // persist path
            Job job = dataLayer.GetJobById(build.JobId);

            string persistPath = _persistPathHelper.GetPath(ContextPluginConfig, job.Key, build.Identifier, "log.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
            if (File.Exists(persistPath))
                return File.ReadAllText(persistPath);

            try
            {
                string log = GetEphemeralBuildLog(build);
                Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
                File.WriteAllText(persistPath, log);
                return log;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download log for build {build.Id}, external id {build.Identifier}", ex);
            }
        }

        public void PollBuildsForJob(Job job) 
        {

        }

        public Build ImportLog(Build build)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            IEnumerable<Build> buildsWithNoLog = dataLayer.GetBuildsWithNoLog(job);

            if (!ResourceHelper.ResourceExists(this.GetType(), $"JSON.builds.{job.Key}.logs.{build.Identifier}.txt"))
                return build;

            string logContent = GetBuildLog(build);
            string logDirectory = Path.Combine(_config.BuildLogsDirectory, GetRandom(), GetRandom());

            Directory.CreateDirectory(logDirectory);
            string logPath = Path.Combine(logDirectory, $"{Guid.NewGuid()}.txt");
            File.WriteAllText(logPath, logContent);

            build.LogPath = logPath;

            return build;
        }

        public Build TryUpdateBuild(Build build)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);

            string path = $"JSON.builds.{job.Key}.builds.json";
            string rawJson;
            if (ResourceHelper.ResourceExists(this.GetType(), path))
                rawJson = ResourceHelper.ReadResourceAsString(this.GetType(), path);
            else
                throw new Exception($"Failed to to read revisions for build {build.Identifier}, job {job.Name}. No data in sandbox");

            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJson);
            IEnumerable<RawBuild> rawBuilds = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RawBuild>>(response.allBuilds.ToString());
            RawBuild rawBuild = rawBuilds.First(b => b.number == build.Identifier.ToString());
            build.EndedUtc = build.StartedUtc.AddMilliseconds(int.Parse(rawBuild.duration));
            build.Status = ConvertBuildStatus(rawBuild.result);

            return build;
        }

        #endregion
    }
}
