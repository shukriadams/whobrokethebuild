﻿using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    public class OrphanRecordHelper
    {
        private readonly PluginProvider _pluginProvider;

        private readonly IDataPlugin _datalayer;

        public OrphanRecordHelper(PluginProvider pluginProvider) 
        {
            _pluginProvider = pluginProvider;
            _datalayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
        }

        public void MergeSourceServers(string fromServerKey, string toServerKey)
        {
            SourceServer fromSourceServer = _datalayer.GetSourceServerByKey(fromServerKey);
            SourceServer toSourceServer = _datalayer.GetSourceServerByKey(toServerKey);

            if (fromSourceServer == null)
                throw new RecordNotFoundException($"ERROR : sourceserver with key \"{fromServerKey}\" not found");

            if (toSourceServer == null)
                throw new RecordNotFoundException($"ERROR : sourceserver with key \"{toSourceServer}\" not found");

            // update job
            foreach (Job job in _datalayer.GetJobs().Where(j => j.SourceServerId == fromSourceServer.Id))
            {
                job.SourceServerId = toSourceServer.Id;
                _datalayer.SaveJob(job);
                ConsoleHelper.WriteLine($"Merged job {job.Key}");
            }

            // update revisions
            foreach (Revision revision in _datalayer.GetRevisionsBySourceServer(fromSourceServer.Id))
            {
                revision.SourceServerId = toSourceServer.Id;
                _datalayer.SaveRevision(revision);
                ConsoleHelper.WriteLine($"Merged revison {revision.Code}");
            }

            _datalayer.DeleteSourceServer(fromSourceServer);
            ConsoleHelper.WriteLine($"Deleted from sourceserver {fromSourceServer.Key}");
        }

        public void MergeBuildServers(string fromServerKey, string toServerKey)
        {
            BuildServer fromBuildServer = _datalayer.GetBuildServerByKey(fromServerKey);
            BuildServer toBuildServer = _datalayer.GetBuildServerByKey(toServerKey);

            if (fromBuildServer == null)
                throw new RecordNotFoundException($"ERROR : build server with key \"{fromServerKey}\" not found");

            if (toBuildServer == null)
                throw new RecordNotFoundException($"ERROR : build server with key \"{toServerKey}\" not found");

            // update job
            foreach (Job job in _datalayer.GetJobs().Where(j => j.BuildServerId == fromBuildServer.Id))
            {
                job.BuildServerId = toBuildServer.Id;
                _datalayer.SaveJob(job);
                ConsoleHelper.WriteLine($"Merged job {job.Key}");
            }

            _datalayer.DeleteBuildServer(fromBuildServer);
            ConsoleHelper.WriteLine($"Deleted from buildserver {fromBuildServer.Key}");
        }

        public void DeleteUser(string key) 
        {
            User record = _datalayer.GetUserByKey(key);
            if (record == null)
                throw new RecordNotFoundException($"User with key {key} not found");

            _datalayer.DeleteUser(record);

            ConsoleHelper.WriteLine($"Deleted user {key}");
        }

        public void DeleteJob(string key) 
        {
            Job record = _datalayer.GetJobByKey(key);
            if (record == null)
                throw new RecordNotFoundException($"Job with key {key} not found");

            _datalayer.DeleteJob(record);

            ConsoleHelper.WriteLine($"Deleted job {key}");
        }

        public void DeleteSourceServer(string key)
        {
            SourceServer record = _datalayer.GetSourceServerByKey(key);
            if (record == null)
                throw new RecordNotFoundException($"Sourceserver with key {key} not found");

            _datalayer.DeleteSourceServer(record);

            ConsoleHelper.WriteLine($"Deleted sourceserver {key}");
        }

        public void DeleteBuildServer(string key)
        {
            BuildServer record = _datalayer.GetBuildServerByKey(key);
            if (record == null)
                throw new RecordNotFoundException($"Buildserver with key {key} not found");

            _datalayer.DeleteBuildServer(record);

            ConsoleHelper.WriteLine($"Deleted buildserver {key}");
        }

        public void MergeUsers(string fromUserKey, string toUserKey)
        {
            User fromUser = _datalayer.GetUserByKey(fromUserKey);
            User toUser = _datalayer.GetUserByKey(toUserKey);

            if (fromUser == null)
                throw new RecordNotFoundException($"ERROR : user with key \"{fromUserKey}\" not found");

            if (toUser == null)
                throw new RecordNotFoundException($"ERROR : user with key \"{toUserKey}\" not found");

            // build involvement
            IEnumerable<BuildInvolvement> buildInvolvements = _datalayer.GetBuildInvolvementByUserId(fromUser.Id);
            foreach(BuildInvolvement buildInvolvement in buildInvolvements)
            {
                buildInvolvement.MappedUserId = toUser.Id;
                _datalayer.SaveBuildInvolement(buildInvolvement);
                ConsoleHelper.WriteLine($"Merged buildInvolvement {buildInvolvement.Id}");
            }

            // session can't be updated, force delete
            IEnumerable<Session> sessions = _datalayer.GetSessionByUserId(fromUser.Id);
            foreach(Session session in sessions)
            {
                _datalayer.DeleteSession(session);
                ConsoleHelper.WriteLine($"Deleted session {session.Id}");
            }

            _datalayer.DeleteUser(fromUser);
            ConsoleHelper.WriteLine($"Deleted from user {fromUser.Key}");
        }
    }
}
