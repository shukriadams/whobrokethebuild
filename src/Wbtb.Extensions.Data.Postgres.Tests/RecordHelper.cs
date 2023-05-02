using System;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.Data.Postgres.Tests
{
    public class RecordHelper : TestBase
    {
        #region BUILDSERVER

        public static BuildServer CreateBuildServer(IDataLayerPlugin postgres)
        {
            BuildServer record = RandomBuildServer();
            return postgres.SaveBuildServer(record);
        }

        public static BuildServer RandomBuildServer()
        {
            return new BuildServer
            {
                Key = RandomString()
            };
        }

        #endregion

        #region SOURCESERVER

        public static SourceServer CreateSourceServer(IDataLayerPlugin postgres)
        {
            SourceServer record = RandomSourceServer();
            return postgres.SaveSourceServer(record);
        }

        public static SourceServer RandomSourceServer()
        {
            return new SourceServer
            {
                Key = RandomString()
            };
        }

        #endregion

        #region JOB

        public static Job CreateJob(IDataLayerPlugin postgres)
        {
            BuildServer buildserver = CreateBuildServer(postgres);
            SourceServer sourceserver = CreateSourceServer(postgres);
            Job job = RandomJob();
            job.SourceServer = sourceserver.Key;
            job.BuildServer = buildserver.Key;

            return postgres.SaveJob(job);
        }

        public static Job RandomJob()
        {
            return new Job
            {
                Key = RandomString()
            };
        }

        #endregion

        #region USER

        public static User CreateUser(IDataLayerPlugin postgres)
        {
            User user = RandomUser();
            return postgres.SaveUser(user);
        }

        public static User RandomUser()
        {
            return new User
            {
                Key = RandomString()
            };
        }

        #endregion

        #region SESSION

        public static Session CreateSession(IDataLayerPlugin postgres)
        {
            Session session = RandomSession();
            User user = CreateUser(postgres);
            session.UserId = user.Key;
            return  postgres.SaveSession(session);
        }

        public static Session RandomSession()
        {
            return new Session
            {
                IP = RandomString(),
                UserAgent = RandomString(),
                UserId = RandomString(),
            };
        }

        #endregion

        #region BUILD

        public static Build RandomBuild()
        { 
            return new Build
            {
                Identifier = RandomString(),
                EndedUtc = new DateTime(),
                Hostname = RandomString(),
                JobId = RandomString(),
                StartedUtc = new DateTime(),
                TriggeringCodeChange = RandomString(),
                TriggeringType = RandomString()
            };
        }

        public static Build CreateBuild(IDataLayerPlugin postgres)
        {
            Job job = CreateJob(postgres);
            Build build = RandomBuild();
            build.JobId = job.Key;
            return postgres.SaveBuild(build);
        }

        #endregion

        #region BUILDLOG

        public static BuildLogParseResult CreateBuildLog(IDataLayerPlugin postgres)
        {
            Build build = CreateBuild(postgres);

            return postgres.SaveBuildLogParseResult(new BuildLogParseResult
            {
                BuildId = build.Id,
                ParsedContent = RandomString(),
                LogParserPlugin = RandomString()
            });
        }

        #endregion

        #region BUILDINVOLVEMENT

        public static BuildInvolvement RandomBuildInvolvement()
        {
            return new BuildInvolvement
            {
                Blame = Blame.Innocent,
                BuildId = "--REQUIRES-BUILDID-FOR-CONSTRAINT",
                IsIgnoredFromBreakHistory = true,
                Comment = RandomString(),
                RevisionCode = RandomString()
            };
        }

        public static BuildInvolvement CreateBuildInvolvement(IDataLayerPlugin postgres)
        {
            Build build = CreateBuild(postgres);
            BuildInvolvement bi = RandomBuildInvolvement();
            bi.BuildId = build.Id;
            return postgres.SaveBuildInvolement(bi);
        }

        #endregion

        #region REVISION

        public static Revision CreateRevision(IDataLayerPlugin postgres)
        {
            SourceServer sourceServer = CreateSourceServer(postgres);

            return postgres.SaveRevision(new Revision
            {
                Code = RandomString(),
                Created = new DateTime(2001, 1, 1),
                Description = RandomString(),
                Files = new string[]{ },
                SourceServerId = sourceServer.Key,
                User = RandomString()
            });
        }

        private static string RandomString()
        { 
            return Guid.NewGuid().ToString();
        }

        #endregion
    }
}
