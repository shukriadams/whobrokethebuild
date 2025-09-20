using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    public class ConfigurationBuilder : IDisposable
    {
        #region FIELDS

        private readonly Configuration _config;
        private readonly PluginProvider _pluginProvider;
        private readonly IDataPlugin _datalayer;
        private readonly Logger _logger;
        #endregion

        #region CTORS

        public ConfigurationBuilder(Configuration config, PluginProvider pluginProvider, Logger logger) 
        {
            _config = config;
            _pluginProvider = pluginProvider;
            _datalayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            _logger = logger;
        }

        #endregion

        #region METHODS

        public void Dispose() 
        {
            _datalayer.Dispose();
        }

        public void TransactionStart() 
        {
            _datalayer.TransactionStart();
        }

        public void TransactionCommit()
        {
            _datalayer.TransactionCommit();
        }

        public void TransactionCancel()
        {
            _datalayer.TransactionCancel();
        }

        public IEnumerable<string> FindOrphans()
        {
            IList<string> errors = new List<string>();

            // source servers
            IEnumerable<SourceServer> existingSourceServers = _datalayer.GetSourceServers();
            foreach (SourceServer existingSourceServer in existingSourceServers)
                if (!_config.SourceServers.Where(s => s.Key == existingSourceServer.Key).Any()) 
                {
                    errors.Add($"The data store contains a source server with key {existingSourceServer.Key} that does not exist in config. This should deleted or merged with a valid object.");
                    foreach(Job job in _datalayer.GetJobs().Where(j => j.SourceServerId == existingSourceServer.Id))
                        errors.Add($"This source server is referenced by job {job.Key}.");
                }

            // buildservers
            IEnumerable<BuildServer> existingBuildServers = _datalayer.GetBuildServers();
            foreach (BuildServer existingBuildServer in existingBuildServers)
                if (!_config.BuildServers.Where(s => s.Key == existingBuildServer.Key).Any()) 
                {
                    errors.Add($"The data store contains a build server with key {existingBuildServer.Key} that does not exist in config. This should deleted or merged with a valid object.");
                    foreach (Job job in _datalayer.GetJobs().Where(j => j.BuildServerId == existingBuildServer.Id))
                        errors.Add($"This build server is referenced by job {job.Key}.");
                }

            // jobs
            IEnumerable<Job> existingJobs = _datalayer.GetJobs();
            foreach (Job existingJob in existingJobs)
            {
                BuildServer buildserver = _datalayer.GetBuildServerById(existingJob.BuildServerId);
                BuildServer buildServerConfig = _config.BuildServers.SingleOrDefault(b => b.Key == buildserver.Key);
                if (buildServerConfig == null)
                {
                    errors.Add($"Job {existingJob.Key} is parented to a build server with {buildserver.Key}, but this build server does not exist in config.");
                    continue;
                }

                if (!buildServerConfig.Jobs.Where(s => s.Key == existingJob.Key).Any())
                    errors.Add($"The data store contains a job with key {existingJob.Key} that does not exist in config under expected build server {buildServerConfig.Key}.");
            }

            // users
            IEnumerable<User> existingUsers = _datalayer.GetUsers();
            foreach (User existingUser in existingUsers)
                if (!_config.Users.Where(s => s.Key == existingUser.Key).Any())
                    errors.Add($"The data store contains a user with key {existingUser.Key} that does not exist in config. This should deleted or merged with a valid object.");

            return errors;
        }

        public void InjectBuildServers()
        { 
            foreach(BuildServer buildServerConfig in _config.BuildServers)
            {
                BuildServer buildserver = _datalayer.GetBuildServerByKey(buildServerConfig.Key);
                
                // update if key changed
                if (buildserver == null && !string.IsNullOrEmpty(buildServerConfig.KeyPrev)) 
                {
                    BuildServer previousCheck = _datalayer.GetBuildServerByKey(buildServerConfig.KeyPrev);
                    if (previousCheck != null) 
                    {
                        previousCheck.Key = buildServerConfig.Key;
                        _datalayer.SaveBuildServer(previousCheck);
                        buildserver = previousCheck;

                        _logger.Status($"BuildServer key changed from {buildServerConfig.KeyPrev} to {buildServerConfig.Key}");
                    }
                }

                if (buildserver == null)
                {
                    buildserver = _datalayer.SaveBuildServer(new BuildServer
                    {
                        Key = buildServerConfig.Key,
                        Name = string.IsNullOrEmpty(buildServerConfig.Name) ? buildServerConfig.Key : buildServerConfig.Name,
                        Plugin = buildServerConfig.Plugin,
                        Description = buildServerConfig.Description
                    });

                    _logger.Status($"SETUP : Created BuildServer {buildServerConfig.Key}", 1);
                } 

                IEnumerable<Job> jobs = _datalayer.GetJobsByBuildServerId(buildserver.Id);

                foreach (Job jobConfig in buildServerConfig.Jobs)
                {
                    Job job = _datalayer.GetJobByKey(jobConfig.Key);
                    SourceServer sourceServer = null;
                    if (!string.IsNullOrEmpty(jobConfig.SourceServer))
                        sourceServer = _datalayer.GetSourceServerByKey(jobConfig.SourceServer);

                    if (job == null)
                    {
                        job = _datalayer.SaveJob(new Job
                        {
                            Key = jobConfig.Key,
                            BuildServerId = buildserver.Id,
                            SourceServerId = sourceServer == null ? null : sourceServer.Id,
                            Name = string.IsNullOrEmpty(jobConfig.Name) ? jobConfig.Key : jobConfig.Name,
                            Description = jobConfig.Description
                        });

                        _logger.Status($"SETUP : Created Job {jobConfig.Key} under build server {buildServerConfig.Key}");
                    }
                    else 
                    {
                        _logger.Status($"VERIFIED : job {job.Key} found");
                    }

                    IBuildServerPlugin buildServerPlugin = _pluginProvider.GetByKey(buildserver.Plugin) as IBuildServerPlugin;

                    // NOTE : if ImportCount is not set and WBTB state has accumulated many builds, this next block will be very slow as it will 
                    // check each build and queue for reimport if not in db.
                    IEnumerable<Build> builds = buildServerPlugin.GetAllCachedBuilds(job).Take(job.ImportCount);

                    _logger.Status($"Checking {builds.Count()} builds", 1);

                    foreach (Build build in builds)
                    {
                        if (_datalayer.GetBuildByKey(job.Id, build.Key) != null)
                            continue;

                        build.JobId = job.Id;
                        build.EndedUtc = null; // force end to null, let daemons process them
                        build.SetUniquePublicIdentifier(job);
                        _datalayer.SaveBuild(build);

                        // create process order for build
                        _datalayer.SaveDaemonTask(new DaemonTask
                        {
                            Stage = (int)ProcessStages.BuildEnd,
                            Src = this.GetType().Name,
                            BuildId = build.Id
                        });

                        _logger.Status($"Imported build {build.Key} under job {job.Name}");
                    }
                }

                _logger.Status($"Done verifying jobs", 1);
            }
        }

        public void InjectUsers()
        {
            foreach (User userConfig in _config.Users)
            {
                User user = _datalayer.GetUserByKey(userConfig.Key);
                if (user == null && !string.IsNullOrEmpty(userConfig.KeyPrev))
                {
                    User previousCheck = _datalayer.GetUserByKey(userConfig.KeyPrev);
                    if (previousCheck != null)
                    {
                        previousCheck.Key = userConfig.Key;
                        _datalayer.SaveUser(previousCheck);
                        user = previousCheck;

                        _logger.Status($"BuildServer key changed from {userConfig.KeyPrev} to {userConfig.Key}");
                    }
                }

                if (user == null) 
                {
                    _datalayer.SaveUser(new User
                    {
                        Key = userConfig.Key,
                        Name = string.IsNullOrEmpty(userConfig.Name) ? userConfig.Key : userConfig.Name,
                        Description = userConfig.Description
                    });

                    _logger.Status($"SETUP : Created User  {userConfig.Key}");

                }
            }
        }

        public void InjectSourceServers()
        {
            foreach (SourceServer sourceServerConfig in _config.SourceServers)
            {
                SourceServer sourceServer = _datalayer.GetSourceServerByKey(sourceServerConfig.Key);
                if (sourceServer == null && !string.IsNullOrEmpty(sourceServerConfig.KeyPrev))
                {
                    SourceServer previousCheck = _datalayer.GetSourceServerByKey(sourceServerConfig.KeyPrev);
                    if (previousCheck != null)
                    {
                        previousCheck.Key = sourceServerConfig.Key;
                        _datalayer.SaveSourceServer(previousCheck);
                        sourceServer = previousCheck;

                        _logger.Status($"BuildServer key changed from {sourceServerConfig.KeyPrev} to {sourceServerConfig.Key}");
                    }
                }

                if (sourceServer == null) 
                {
                    _datalayer.SaveSourceServer(new SourceServer
                    {
                        Key = sourceServerConfig.Key,
                        Name = string.IsNullOrEmpty(sourceServerConfig.Name) ? sourceServerConfig.Key : sourceServerConfig.Name,
                        Plugin = sourceServerConfig.Plugin,
                        Description = sourceServerConfig.Description
                    });

                    _logger.Status($"SETUP : Created Source Server {sourceServerConfig.Key}");
                }
            }
        }

    }

    #endregion
}
