using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Configuration
{
    public class ConfigurationBuilder
    {

        public static IEnumerable<string> FindOrphans()
        {
            IDataLayerPlugin datalayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            IList<string> errors = new List<string>();

            // source servers
            IEnumerable<SourceServer> existingSourceServers = datalayer.GetSourceServers();
            foreach (SourceServer existingSourceServer in existingSourceServers)
                if (!ConfigKeeper.Instance.SourceServers.Where(s => s.Key == existingSourceServer.Key).Any()) 
                {
                    errors.Add($"The data store contains a source server with key {existingSourceServer.Key} that does not exist in config. This should deleted or merged with a valid object.");
                    foreach(Job job in datalayer.GetJobs().Where(j => j.SourceServerId == existingSourceServer.Id))
                        errors.Add($"This source server is referenced by job {job.Key}.");
                }

            // buildservers
            IEnumerable<BuildServer> existingBuildServers = datalayer.GetBuildServers();
            foreach (BuildServer existingBuildServer in existingBuildServers)
                if (!ConfigKeeper.Instance.BuildServers.Where(s => s.Key == existingBuildServer.Key).Any()) 
                {
                    errors.Add($"The data store contains a build server with key {existingBuildServer.Key} that does not exist in config. This should deleted or merged with a valid object.");
                    foreach (Job job in datalayer.GetJobs().Where(j => j.BuildServerId == existingBuildServer.Id))
                        errors.Add($"This build server is referenced by job {job.Key}.");
                }

            // jobs
            IEnumerable<Job> existingJobs = datalayer.GetJobs();
            foreach (Job existingJob in existingJobs)
            {
                BuildServer buildserver = datalayer.GetBuildServerById(existingJob.BuildServerId);
                BuildServer buildServerConfig = ConfigKeeper.Instance.BuildServers.SingleOrDefault(b => b.Key == buildserver.Key);
                if (buildServerConfig == null)
                {
                    errors.Add($"Job {existingJob.Key} is parented to a build server with {buildserver.Key}, but this build server does not exist in config.");
                    continue;
                }

                if (!buildServerConfig.Jobs.Where(s => s.Key == existingJob.Key).Any())
                    errors.Add($"The data store contains a job with key {existingJob.Key} that does not exist in config under expected build server {buildServerConfig.Key}.");
            }

            // users
            IEnumerable<User> existingUsers = datalayer.GetUsers();
            foreach (User existingUser in existingUsers)
                if (!ConfigKeeper.Instance.Users.Where(s => s.Key == existingUser.Key).Any())
                    errors.Add($"The data store contains a user with key {existingUser.Key} that does not exist in config. This should deleted or merged with a valid object.");

            return errors;
        }

        public static void InjectBuildServers()
        { 
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            List<string> errors = new List<string>();

            foreach(BuildServer buildServerConfig in ConfigKeeper.Instance.BuildServers)
            {
                BuildServer buildserver = dataLayer.GetBuildServerByKey(buildServerConfig.Key);
                
                // update if key changed
                if (buildserver == null && !string.IsNullOrEmpty(buildServerConfig.KeyPrev)) 
                {
                    BuildServer previousCheck = dataLayer.GetBuildServerByKey(buildServerConfig.KeyPrev);
                    if (previousCheck != null) 
                    {
                        previousCheck.Key = buildServerConfig.Key;
                        dataLayer.SaveBuildServer(previousCheck);
                        buildserver = previousCheck;

                        Console.WriteLine($"BuildServer key changed from {buildServerConfig.KeyPrev} to {buildServerConfig.Key}");
                    }
                }

                if (buildserver == null)
                {
                    buildserver = dataLayer.SaveBuildServer(new BuildServer
                    {
                        Key = buildServerConfig.Key,
                        Name = string.IsNullOrEmpty(buildServerConfig.Name) ? buildServerConfig.Key : buildServerConfig.Name,
                        Plugin = buildServerConfig.Plugin,
                        Description = buildServerConfig.Description
                    });

                    Console.WriteLine($"WBTB : SETUP : Created BuildServer {buildServerConfig.Key}");
                } 

                IEnumerable<Job> jobs = dataLayer.GetJobsByBuildServerId(buildserver.Id);

                foreach (Job jobConfig in buildServerConfig.Jobs)
                {
                    Job job = dataLayer.GetJobByKey(jobConfig.Key);
                    if (job != null)
                    {
                        Console.WriteLine($"VERIFIED : job {job.Key} found");
                        continue;
                    }

                    if (jobs.Count() == buildServerConfig.Jobs.Count())
                    {
                        errors.Add($"ERROR: job {jobConfig.Key} was not found, but expected number of job ({buildServerConfig.Jobs.Count()}) are present. If you changed config, please manually update the job record to match new config.");
                        continue;
                    }

                    SourceServer sourceServer = dataLayer.GetSourceServerByKey(jobConfig.SourceServer);
                    if (sourceServer == null)
                        throw new ConfigurationException($"Failed to create job \"{jobConfig.Key}\", did not find expected source server \"{jobConfig.SourceServer}\"");

                    job = dataLayer.SaveJob(new Job
                    {
                        Key = jobConfig.Key,
                        BuildServerId = buildserver.Id,
                        SourceServerId = sourceServer.Id,
                        Name = string.IsNullOrEmpty(jobConfig.Name) ? jobConfig.Key : jobConfig.Name,
                        Description = jobConfig.Description
                    });

                    IBuildServerPlugin buildServerPlugin = PluginProvider.GetByKey(buildserver.Plugin) as IBuildServerPlugin;
                    buildServerPlugin.ImportAllCachedBuilds(job);

                    Console.WriteLine($"WBTB : SETUP : Created Job {jobConfig.Key} under build server {buildServerConfig.Key}");
                }
            }

        }

        public static void InjectUsers()
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            foreach (User userConfig in ConfigKeeper.Instance.Users)
            {
                User user = dataLayer.GetUserByKey(userConfig.Key);
                if (user == null && !string.IsNullOrEmpty(userConfig.KeyPrev))
                {
                    User previousCheck = dataLayer.GetUserByKey(userConfig.KeyPrev);
                    if (previousCheck != null)
                    {
                        previousCheck.Key = userConfig.Key;
                        dataLayer.SaveUser(previousCheck);
                        user = previousCheck;

                        Console.WriteLine($"BuildServer key changed from {userConfig.KeyPrev} to {userConfig.Key}");
                    }
                }

                if (user == null) 
                {
                    dataLayer.SaveUser(new User
                    {
                        Key = userConfig.Key,
                        Name = string.IsNullOrEmpty(userConfig.Name) ? userConfig.Key : userConfig.Name,
                        Description = userConfig.Description
                    });

                    Console.WriteLine($"WBTB : SETUP : Created User  {userConfig.Key}");

                }
            }
        }

        public static void InjectSourceServers()
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            foreach (SourceServer sourceServerConfig in ConfigKeeper.Instance.SourceServers)
            {
                SourceServer sourceServer = dataLayer.GetSourceServerByKey(sourceServerConfig.Key);
                if (sourceServer == null && !string.IsNullOrEmpty(sourceServerConfig.KeyPrev))
                {
                    SourceServer previousCheck = dataLayer.GetSourceServerByKey(sourceServerConfig.KeyPrev);
                    if (previousCheck != null)
                    {
                        previousCheck.Key = sourceServerConfig.Key;
                        dataLayer.SaveSourceServer(previousCheck);
                        sourceServer = previousCheck;

                        Console.WriteLine($"BuildServer key changed from {sourceServerConfig.KeyPrev} to {sourceServerConfig.Key}");
                    }
                }

                if (sourceServer == null) 
                {
                    dataLayer.SaveSourceServer(new SourceServer
                    {
                        Key = sourceServerConfig.Key,
                        Name = string.IsNullOrEmpty(sourceServerConfig.Name) ? sourceServerConfig.Key : sourceServerConfig.Name,
                        Plugin = sourceServerConfig.Plugin,
                        Description = sourceServerConfig.Description
                    });

                    Console.WriteLine($"WBTB : SETUP : Created Source Server {sourceServerConfig.Key}");
                }
            }
        }

    }
}
