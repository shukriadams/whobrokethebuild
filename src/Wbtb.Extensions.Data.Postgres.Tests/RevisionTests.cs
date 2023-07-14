using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Xunit;

namespace Wbtb.Extensions.Data.Postgres.Tests
{
    [Collection("Sequential")]
    public class RevisionTests : TestBase
    {
        #region TESTS

        [Fact]
        public void CreatePropertyCheck()
        {
            // create
            SourceServer sourceserver = RecordHelper.CreateSourceServer(this.Postgres);
            Revision record = this.Postgres.SaveRevision(new Revision
            {
                Code = "code123",
                Description = "description123",
                Created = new DateTime(2001,1,1),
                SourceServerId = sourceserver.Key,
                Files = new string[]{ "a/file", "another/file" },
                User = "myuser"
            });

            Assert.NotEqual("0", record.Id);

            Assert.Equal("code123", record.Code);
            Assert.Equal("description123", record.Description);
            Assert.Equal(new DateTime(2001, 1, 1), record.Created);
            Assert.Equal(sourceserver.Key, record.SourceServerId);
            Assert.Equal("myuser", record.User);
            Assert.Equal(2, record.Files.Count());
        }

        [Fact]
        public void Get()
        {
            // create
            Revision record = RecordHelper.CreateRevision(this.Postgres);

            // retrieve
            Revision get = this.Postgres.GetRevisionById(record.Id);
            Assert.NotNull(get);
        }


        [Fact]
        public void Update()
        {
            // create 
            Revision record = RecordHelper.CreateRevision(this.Postgres);

            // retrieve then update
            Revision get = this.Postgres.GetRevisionById(record.Id);
            get.Files = new string[]{ "file1" , "file2", "file3" };
            this.Postgres.SaveRevision(get);

            // retrieve again and ensure update succeeded
            get = this.Postgres.GetRevisionById(record.Id);
            Assert.Equal(3, get.Files.Count());
            Assert.Contains("file1", get.Files);
            Assert.Contains("file2", get.Files);
            Assert.Contains("file3", get.Files);
        }

        [Fact]
        public void ListRevisionsByBuild()
        {
            // create some basic records required by constraints
            Build build = RecordHelper.CreateBuild(this.Postgres);
            Revision rev1 = RecordHelper.CreateRevision(this.Postgres);
            Revision rev2 = RecordHelper.CreateRevision(this.Postgres);

            // create build involvement
            BuildInvolvement bi1 = RecordHelper.RandomBuildInvolvement();
            bi1.BuildId = build.Id;
            bi1.RevisionId = rev1.Id;
            Postgres.SaveBuildInvolement(bi1);

            BuildInvolvement bi2 = RecordHelper.RandomBuildInvolvement();
            bi2.BuildId = build.Id;
            bi2.RevisionId = rev2.Id;
            Postgres.SaveBuildInvolement(bi2);

            // get revisions linked via build involvmenet
            IEnumerable<Revision> revisions = this.Postgres.GetRevisionByBuild(build.Id);
            Assert.Equal(2, revisions.Count());
            Assert.Contains(revisions, r => r.Id == rev1.Id);
            Assert.Contains(revisions, r => r.Id == rev2.Id);
        }

        [Fact]
        public void Delete()
        {
            // create
            Revision record = RecordHelper.CreateRevision(this.Postgres);

            // delete
            this.Postgres.DeleteRevision(record);

            // ensure delete succeeded
            Revision get = this.Postgres.GetRevisionById(record.Id);
            Assert.Null(get);
        }

        #endregion
    }
}

