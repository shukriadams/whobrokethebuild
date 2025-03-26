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

        public new void Diagnose() 
        {
            IDataPlugin data = _di.Resolve<IDataPlugin>();
            var records = data.GetBuildServers();
        }

        public ReachAttemptResult AttemptReach(Core.Common.BuildServer contextServer)
        {
            // each job should have RemoteKey config item
            foreach (Job job in contextServer.Jobs)
            {
                if (job.Config == null)
                    throw new ConfigurationException($"Job {job.Key} on buildServer {contextServer.Key} is missing Config node");

                if (!job.Config.Any(c => c.Key == "RemoteKey"))
                    throw new ConfigurationException($"Job {job.Key} on buildServer {contextServer.Key} is missing Config \"RemoteKey\"");

                if (!job.Config.Any(c => c.Key == "Interval"))
                    throw new ConfigurationException($"Job {job.Key} on buildServer {contextServer.Key} is missing Config \"Interval\". This is for date-over-time. Add default value 0.");

            }

            return new ReachAttemptResult { Reachable = true };
        }

        public void AttemptReachJob(Core.Common.BuildServer buildServer, Job job)
        {
            // do nothing, assume always passes
        }

        #endregion

        #region METHODS

        void IBuildServerPlugin.VerifyBuildServerConfig(Core.Common.BuildServer buildServer)
        {

        }

        private static PluginConfig GetThisConfig()
        {
            return null;
        }

        string IBuildServerPlugin.GetBuildUrl(Core.Common.BuildServer contextServer, Build build)
        {
            return string.Empty;
        }

        IEnumerable<string> IBuildServerPlugin.ListRemoteJobsCanonical(Core.Common.BuildServer buildServer)
        {
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

        BuildRevisionsRetrieveResult IBuildServerPlugin.GetRevisionsInBuild(Build build)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);

            var remotekey = job.Config.FirstOrDefault(c => c.Key == "RemoteKey");
            string rawJson = null;
            string persistPath = _persistPathHelper.GetPath(ContextPluginConfig, job.Key, build.Key, "revisions.json");
            string path = $"JSON.builds.{remotekey.Value}.build_{build.Key}_revisions.json";

            if (ResourceHelper.ResourceExists(this.GetType(), path))
            {
                rawJson = ResourceHelper.ReadResourceAsString(this.GetType(), path);
                Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
                File.WriteAllText(persistPath, rawJson);
            }
            else
                ConsoleHelper.WriteLine($"Failed to to read revisions for build {build.Key}, job {remotekey.Value}. No data in sandbox");
            
            if (rawJson == null)
                return new BuildRevisionsRetrieveResult { Result = "revisions not available in sandbox" };

            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJson);
            IEnumerable<RawRevision> rawRevisions = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RawRevision>>(response.changeSet.items.ToString());
            return new BuildRevisionsRetrieveResult { Revisions = rawRevisions.Select(r => r.commitId) , Success = true };
        }

        IEnumerable<Build> IBuildServerPlugin.GetLatesBuilds(Job job, int take)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            Core.Common.BuildServer buildServer = dataLayer.GetBuildServerById(job.BuildServerId);
            string interval = job.Config.First(r => r.Key == "Interval").Value.ToString();

            var remoteKey = job.Config.FirstOrDefault(c => c.Key == "RemoteKey");
            string resourcePath = $"JSON.builds.{remoteKey.Value}.builds.json";

            if (!ResourceHelper.ResourceExists(this.GetType(), resourcePath))
                throw new Exception($"Failed to import builds, sandbox job {remoteKey.Value} does not exist. Create a static directory, add content and ensure they are marked as embedded resource.");

            string rawJson = ResourceHelper.ReadResourceAsString(this.GetType(), resourcePath);
            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJson);
            IEnumerable<RawBuild> rawBuilds = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RawBuild>>(response.allBuilds.ToString());

            // dev feature : To simulate passing of time on build server, add the optional
            //     "dev_isLatest" : true
            // property in the builds.json file job is importing from. Importing will stop at the first build in file that has this flag.
            // moving flag requires app rebuild, it's not great, but it's what we have
            IList<Build> builds = new List<Build>();
            foreach (RawBuild rawBuild in rawBuilds) 
            {
                builds.Add(new Build
                {
                    Key = rawBuild.number,
                    Hostname = rawBuild.builtOn,
                    StartedUtc = UnixTimeStampToDateTime(rawBuild.timestamp),
                    Status = ConvertBuildStatus(rawBuild.result)
                });

                if (rawBuild.dev_isLatest == "true")
                    break;
            }

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

        IEnumerable<Build> IBuildServerPlugin.GetAllCachedBuilds(Job job)
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
                    Key = rawBuild.number,
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

        string IBuildServerPlugin.GetEphemeralBuildLog(Build build)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            // persist path
            Job job = dataLayer.GetJobById(build.JobId);
            var remotekey = job.Config.FirstOrDefault(c => c.Key == "RemoteKey");

            string log = ResourceHelper.ReadResourceAsString(this.GetType(), $"JSON.builds.{remotekey.Value}.logs.{build.Key}.txt");
            return log;
        }

        private string GetBuildLog(Build build)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            // persist path
            Job job = dataLayer.GetJobById(build.JobId);

            string persistPath = _persistPathHelper.GetPath(ContextPluginConfig, job.Key, build.Key, "log.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
            if (File.Exists(persistPath))
                return File.ReadAllText(persistPath);

            try
            {
                string log = ((IBuildServerPlugin)this).GetEphemeralBuildLog(build);
                Directory.CreateDirectory(Path.GetDirectoryName(persistPath));
                File.WriteAllText(persistPath, log);
                return log;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download log for build {build.Id}, external id {build.Key}", ex);
            }
        }

        void IBuildServerPlugin.PollBuildsForJob(Job job) 
        {
            // not used in this implementation
        }

        BuildLogRetrieveResult IBuildServerPlugin.ImportLog(Build build)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            var remotekey = job.Config.FirstOrDefault(c => c.Key == "RemoteKey");

            if (build.Key == "39" && job.Name.ToLower().Contains("skunk"))
                throw new Exception("Log import failed deliberately on build 39 for skunkwords. This is to test daemontask error handling.");

            if (!ResourceHelper.ResourceExists(this.GetType(), $"JSON.builds.{remotekey.Value}.logs.{build.Key}.txt"))
                return new BuildLogRetrieveResult { Result = "Build log does not exist" };

            string logContent = GetBuildLog(build);
            string logPath = Build.GetLogPath(_config, job, build);

            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            File.WriteAllText(logPath, logContent);

            return new BuildLogRetrieveResult { Success = true  };
        }

        Build IBuildServerPlugin.TryUpdateBuild(Build build)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            var remotekey = job.Config.FirstOrDefault(c => c.Key == "RemoteKey");


            string path = $"JSON.builds.{remotekey.Value}.builds.json";
            string rawJson;
            if (ResourceHelper.ResourceExists(this.GetType(), path))
                rawJson = ResourceHelper.ReadResourceAsString(this.GetType(), path);
            else
                throw new Exception($"Failed to to read revisions for build {build.Key}, job {job.Name}. No data in sandbox");

            dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject(rawJson);
            IEnumerable<RawBuild> rawBuilds = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<RawBuild>>(response.allBuilds.ToString());
            RawBuild rawBuild = rawBuilds.First(b => b.number == build.Key.ToString());
            build.EndedUtc = build.StartedUtc.AddMilliseconds(int.Parse(rawBuild.duration));
            build.Status = ConvertBuildStatus(rawBuild.result);

            return build;
        }

        #endregion
    }
}
