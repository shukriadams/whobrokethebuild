using System;
using System.Collections.Generic;
using Xunit;

namespace Wbtb.Extensions.Data.Postgres.Tests
{
    [Collection("Sequential")]
    public class BuildLogTests : TestBase
    {
        #region TESTS

        [Fact]
        public void CreatePropertyCheck()
        {
            // create
            Core.Common.Build build = RecordHelper.CreateBuild(this.Postgres);

            Core.Common.BuildLogParseResult record = this.Postgres.SaveBuildLogParseResult(new Core.Common.BuildLogParseResult
            {
                BuildId = build.Id,
                ParsedContent = "mycontent",
            });

            Assert.Equal(build.Id, record.BuildId);
            Assert.Equal("mycontent", record.ParsedContent);
        }

        [Fact]
        public void Get()
        {
            // create
            Core.Common.BuildLogParseResult record = RecordHelper.CreateBuildLog(this.Postgres);

            // retrieve
            IEnumerable<Core.Common.BuildLogParseResult> get = this.Postgres.GetBuildLogParseResultsByBuildId(record.BuildId);
            Assert.NotEmpty(get);
        }


        [Fact]
        public void Update()
        {
            // create 
            Core.Common.BuildLogParseResult record = RecordHelper.CreateBuildLog(this.Postgres);

            // retrieve then update
            IEnumerable<Core.Common.BuildLogParseResult> get = this.Postgres.GetBuildLogParseResultsByBuildId(record.BuildId);

            Assert.False(true); // reimplement test
        }

        [Fact]
        public void Delete()
        {
            // create
            Core.Common.BuildLogParseResult record = RecordHelper.CreateBuildLog(this.Postgres);

            // delete
            this.Postgres.DeleteBuildLogParseResult(record);

            // ensure delete succeeded
            IEnumerable<Core.Common.BuildLogParseResult> get = this.Postgres.GetBuildLogParseResultsByBuildId(record.BuildId);
            Assert.Empty(get);
        }

        #endregion
    }
}
