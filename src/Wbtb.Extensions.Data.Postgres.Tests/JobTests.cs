using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Xunit;

namespace Wbtb.Extensions.Data.Postgres.Tests
{
    [Collection("Sequential")]
    public class JobTests : TestBase
    {
        #region TESTS

        [Fact]
        public void CreatePropertyCheck()
        {
            // create
            BuildServer buildserver = RecordHelper.CreateBuildServer(this.Postgres);
            SourceServer sourceServer = RecordHelper.CreateSourceServer(this.Postgres);

            Job record = this.Postgres.SaveJob(new Job
            {
                Key = "my job",
                BuildServer = buildserver.Key,
                SourceServer = sourceServer.Key
            });

            Assert.NotEqual("0", record.Key);
            Assert.Equal("my job", record.Key);
        }

        [Fact]
        public void Get()
        {
            // create
            Job record = RecordHelper.CreateJob(this.Postgres);

            // retrieve
            Job get = this.Postgres.GetJobById(record.Key);
            Assert.NotNull(get);
        }


        [Fact]
        public void Update()
        {
            // create 
            Job record = RecordHelper.CreateJob(this.Postgres);

            // retrieve then update
            Job get = this.Postgres.GetJobById(record.Key);
            get.Key = "new name";
            this.Postgres.SaveJob(get);

            // retrieve again and ensure update succeeded
            get = this.Postgres.GetJobById(record.Key);
            Assert.Equal("new name", get.Key);
        }

        [Fact]
        public void Delete()
        {
            // create
            Job record = RecordHelper.CreateJob(this.Postgres);

            // delete
            this.Postgres.DeleteJob(record);

            // ensure delete succeeded
            Job get = this.Postgres.GetJobById(record.Key);
            Assert.Null(get);
        }

        [Fact]
        public void GetAll()
        {
            // create 
            BuildServer buildserver = RecordHelper.CreateBuildServer(this.Postgres);
            SourceServer sourceServer = RecordHelper.CreateSourceServer(this.Postgres);
            Job record1 = this.Postgres.SaveJob(new Job { Key = "job1", BuildServer = buildserver.Key, SourceServer = sourceServer.Key });
            Job record2 = this.Postgres.SaveJob(new Job { Key = "job2", BuildServer = buildserver.Key, SourceServer = sourceServer.Key });


            IEnumerable<Job> records = this.Postgres.GetJobsByBuildServerId(buildserver.Id);
            Assert.Equal(2, records.Count());
            Assert.Contains(records, r => r.Key == record1.Key);
            Assert.Contains(records, r => r.Key == record2.Key);
        }

        #endregion
    }
}
