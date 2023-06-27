using System;
using System.Linq;
using Wbtb.Core.Common;
using Xunit;

namespace Wbtb.Extensions.Data.Postgres.Tests
{
    [Collection("Sequential")]
    public class BuildTests : TestBase
    {
        #region TESTS

        [Fact]
        public void CreatePropertyCheck()
        {
            // create
            Job job = RecordHelper.CreateJob(this.Postgres);
                
            Build record = this.Postgres.SaveBuild(new Build
            {
                Identifier = "mybuildId",
                EndedUtc = new DateTime(2001,1,1),
                Hostname = "myhostname",
                JobId = job.Key,
                StartedUtc = new DateTime(2000, 1, 1),
                Status = BuildStatus.InProgress,
                TriggeringCodeChange = "myRevision",
                TriggeringType = "someEvent"
            });

            Assert.NotEqual("0", record.Id);

            Assert.Equal("mybuildId", record.Identifier);
            Assert.Equal(new DateTime(2001, 1, 1), record.EndedUtc);
            Assert.Equal("myhostname", record.Hostname);
            Assert.Equal(job.Key, record.JobId);
            Assert.Equal(new DateTime(2000, 1, 1), record.StartedUtc);
            Assert.Equal(BuildStatus.InProgress, record.Status);
            Assert.Equal("myRevision", record.TriggeringCodeChange);
            Assert.Equal("someEvent", record.TriggeringType);
        }

        [Fact]
        public void Get()
        {
            // create
            Build record = RecordHelper.CreateBuild(this.Postgres);

            // retrieve
            Build get = this.Postgres.GetBuildById(record.Id);
            Assert.NotNull(get);
        }


        [Fact]
        public void Update()
        {
            // create 
            Build record = RecordHelper.CreateBuild(this.Postgres);

            // retrieve then update
            Build get = this.Postgres.GetBuildById(record.Id);
            get.Hostname = "a new host";
            this.Postgres.SaveBuild(get);

            // retrieve again and ensure update succeeded
            get = this.Postgres.GetBuildById(record.Id);
            Assert.Equal("a new host", get.Hostname);
        }

        [Fact]
        public void Delete()
        {
            // create
            Build record = RecordHelper.CreateBuild(this.Postgres);

            // delete
            this.Postgres.DeleteBuild(record);

            // ensure delete succeeded
            Build get = this.Postgres.GetBuildById(record.Id);
            Assert.Null(get);
        }

        [Fact]
        public void Page()
        {
            // create 
            Job job1 = RecordHelper.CreateJob(this.Postgres);
            Job job2 = RecordHelper.CreateJob(this.Postgres);
            Build record1 = this.Postgres.SaveBuild(new Build { JobId = job1.Key, Identifier = "a", TriggeringCodeChange = "a", TriggeringType = "a", Hostname = "a" });
            Build record2 = this.Postgres.SaveBuild(new Build { JobId = job1.Key, Identifier = "b", TriggeringCodeChange = "a", TriggeringType = "a", Hostname = "a" });
            Build record3 = this.Postgres.SaveBuild(new Build { JobId = job2.Key, Identifier = "c", TriggeringCodeChange = "a", TriggeringType = "a", Hostname = "a" });


            PageableData<Build> page = this.Postgres.PageBuildsByJob(job1.Key, 0, 2, false);
            Assert.Equal(2, page.TotalItemCount);
            Assert.Equal(2, page.Items.Count());
            Assert.Contains(page.Items, r => r.Id == record1.Id);
            Assert.Contains(page.Items, r => r.Id == record2.Id);
        }

        #endregion
    }
}

