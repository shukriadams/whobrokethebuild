using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Xunit;

namespace Wbtb.Extensions.Data.Postgres.Tests
{
    [Collection("Sequential")]
    public class BuildInvolvementTests : TestBase
    {
        #region TESTS

        [Fact]
        public void CreatePropertyCheck()
        {
            // create
            Build build = RecordHelper.CreateBuild(this.Postgres);

            BuildInvolvement record = this.Postgres.SaveBuildInvolement(new BuildInvolvement
            {
                BuildId = build.Id,
                Comment = "mycomment",
                IsIgnoredFromBreakHistory = true,
                RevisionCode = "myrevision"
            });

            Assert.NotEqual("0", record.Id);

            Assert.Equal("mycomment", record.Comment);
            Assert.True(record.IsIgnoredFromBreakHistory);
            Assert.Equal("myrevision", record.RevisionCode);
        }

        [Fact]
        public void Get()
        {
            // create
            BuildInvolvement record = RecordHelper.CreateBuildInvolvement(this.Postgres);

            // retrieve
            BuildInvolvement get = this.Postgres.GetBuildInvolvementById(record.Id);
            Assert.NotNull(get);
        }


        [Fact]
        public void Update()
        {
            // create 
            BuildInvolvement record = RecordHelper.CreateBuildInvolvement(this.Postgres);

            // retrieve then update
            Core.Common.BuildInvolvement get = this.Postgres.GetBuildInvolvementById(record.Id);
            get.Comment = "newcomment";
            this.Postgres.SaveBuildInvolement(get);

            // retrieve again and ensure update succeeded
            get = this.Postgres.GetBuildInvolvementById(record.Id);
            Assert.Equal("newcomment", get.Comment);
        }

        [Fact]
        public void Delete()
        {
            // create
            BuildInvolvement record = RecordHelper.CreateBuildInvolvement(this.Postgres);

            // delete
            this.Postgres.DeleteBuildInvolvement(record);

            // ensure delete succeeded
            BuildInvolvement get = this.Postgres.GetBuildInvolvementById(record.Id);
            Assert.Null(get);
        }

        [Fact]
        public void GetByBuild()
        {
            Build build = RecordHelper.CreateBuild(this.Postgres);
            Build build2 = RecordHelper.CreateBuild(this.Postgres);

            BuildInvolvement bi1 =  this.Postgres.SaveBuildInvolement(new BuildInvolvement{ BuildId = build.Id, RevisionCode = "r1" });
            BuildInvolvement bi2 = this.Postgres.SaveBuildInvolement(new BuildInvolvement { BuildId = build.Id, RevisionCode = "r2" });
            BuildInvolvement bi_other = this.Postgres.SaveBuildInvolement(new BuildInvolvement { BuildId = build2.Id, RevisionCode = "r3" });

            IEnumerable<BuildInvolvement> buildInvolvements = this.Postgres.GetBuildInvolvementsByBuild(build.Id);
            Assert.Equal(2, buildInvolvements.Count());
            Assert.Contains(buildInvolvements, r => r.Id == bi1.Id);
            Assert.Contains(buildInvolvements, r => r.Id == bi2.Id);
        }

        #endregion
    }
}

