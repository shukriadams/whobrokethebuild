using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.CLI
{
    public class OrphanRecordHelper
    {
        private readonly PluginProvider _pluginProvider;
        
        public OrphanRecordHelper(PluginProvider pluginProvider) 
        {
            _pluginProvider = pluginProvider;
        }

        public void MergeSourceServers(string fromServerKey, string toServerKey)
        {
            IDataLayerPlugin datalayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            SourceServer fromSourceServer = datalayer.GetSourceServerByKey(fromServerKey);
            SourceServer toSourceServer = datalayer.GetSourceServerByKey(toServerKey);

            if (fromSourceServer == null)
                throw new RecordNotFoundException($"ERROR : sourceserver with key \"{fromServerKey}\" not found");

            if (toSourceServer == null)
                throw new RecordNotFoundException($"ERROR : sourceserver with key \"{toSourceServer}\" not found");

            // update job
            foreach (Job job in datalayer.GetJobs().Where(j => j.SourceServerId == fromSourceServer.Id))
            {
                job.SourceServerId = toSourceServer.Id;
                datalayer.SaveJob(job);
                Console.WriteLine($"Merged job {job.Key}");
            }

            // update revisions
            foreach (Revision revision in datalayer.GetRevisionsBySourceServer(fromSourceServer.Id))
            {
                revision.SourceServerId = toSourceServer.Id;
                datalayer.SaveRevision(revision);
                Console.WriteLine($"Merged revison {revision.Code}");
            }

            datalayer.DeleteSourceServer(fromSourceServer);
            Console.WriteLine($"Deleted from sourceserver {fromSourceServer.Key}");
        }

        public void MergeBuildServers(string fromServerKey, string toServerKey)
        {
            IDataLayerPlugin datalayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            BuildServer fromBuildServer = datalayer.GetBuildServerByKey(fromServerKey);
            BuildServer toBuildServer = datalayer.GetBuildServerByKey(toServerKey);

            if (fromBuildServer == null)
                throw new RecordNotFoundException($"ERROR : build server with key \"{fromServerKey}\" not found");

            if (toBuildServer == null)
                throw new RecordNotFoundException($"ERROR : build server with key \"{toServerKey}\" not found");

            // update job
            foreach (Job job in datalayer.GetJobs().Where(j => j.BuildServerId == fromBuildServer.Id))
            {
                job.BuildServerId = toBuildServer.Id;
                datalayer.SaveJob(job);
                Console.WriteLine($"Merged job {job.Key}");
            }

            datalayer.DeleteBuildServer(fromBuildServer);
            Console.WriteLine($"Deleted from buildserver {fromBuildServer.Key}");
        }

        public void DeleteUser(string key) 
        {
            IDataLayerPlugin datalayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            User record = datalayer.GetUserByKey(key);
            if (record == null)
                throw new RecordNotFoundException($"User with key {key} not found");

            datalayer.DeleteUser(record);

            Console.WriteLine($"Deleted user {key}");
        }

        public void DeleteSourceServer(string key)
        {
            IDataLayerPlugin datalayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            SourceServer record = datalayer.GetSourceServerByKey(key);
            if (record == null)
                throw new RecordNotFoundException($"Sourceserver with key {key} not found");

            datalayer.DeleteSourceServer(record);

            Console.WriteLine($"Deleted sourceserver {key}");
        }

        public void DeleteBuildServer(string key)
        {
            IDataLayerPlugin datalayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            BuildServer record = datalayer.GetBuildServerByKey(key);
            if (record == null)
                throw new RecordNotFoundException($"Buildserver with key {key} not found");

            datalayer.DeleteBuildServer(record);

            Console.WriteLine($"Deleted buildserver {key}");
        }

        public void MergeUsers(string fromUserKey, string toUserKey)
        {
            IDataLayerPlugin datalayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            User fromUser = datalayer.GetUserByKey(fromUserKey);
            User toUser = datalayer.GetUserByKey(toUserKey);

            if (fromUser == null)
                throw new RecordNotFoundException($"ERROR : user with key \"{fromUserKey}\" not found");

            if (toUser == null)
                throw new RecordNotFoundException($"ERROR : user with key \"{toUserKey}\" not found");

            // build involvement
            IEnumerable<BuildInvolvement> buildInvolvements = datalayer.GetBuildInvolvementByUserId(fromUser.Id);
            foreach(BuildInvolvement buildInvolvement in buildInvolvements)
            {
                buildInvolvement.MappedUserId = toUser.Id;
                datalayer.SaveBuildInvolement(buildInvolvement);
                Console.WriteLine($"Merged buildInvolvement {buildInvolvement.Id}");
            }

            // session can't be updated, force delete
            IEnumerable<Session> sessions = datalayer.GetSessionByUserId(fromUser.Id);
            foreach(Session session in sessions)
            { 
                datalayer.DeleteSession(session);
                Console.WriteLine($"Deleted session {session.Id}");
            }

            datalayer.DeleteUser(fromUser);
            Console.WriteLine($"Deleted from user {fromUser.Key}");
        }
    }
}
