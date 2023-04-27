using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Xunit;

namespace Wbtb.Extensions.Data.Postgres.Tests
{
    [Collection("Sequential")]
    public class BuildServerTests : TestBase
    {
        #region TESTS

        [Fact]
        public void CreatePropertyCheck()
        {
            // create
            BuildServer record = this.Postgres.SaveBuildServer(new BuildServer
            {
                Key = "my build"
            });

            Assert.NotEqual("0", record.Key);
            Assert.Equal("my build", record.Key);
        }

        [Fact]
        public void Get()
        {
            // create
            BuildServer record = RecordHelper.CreateBuildServer(this.Postgres);

            // retrieve
            BuildServer get = this.Postgres.GetBuildServerById(record.Key);
            Assert.NotNull(get);
        }


        [Fact]
        public void Update()
        {
            // create 
            BuildServer record = RecordHelper.CreateBuildServer(this.Postgres);

            // retrieve then update
            BuildServer get = this.Postgres.GetBuildServerById(record.Key);
            get.Key = "new name";
            this.Postgres.SaveBuildServer(get);

            // retrieve again and ensure update succeeded
            get = this.Postgres.GetBuildServerById(record.Key);
            Assert.Equal("new name", get.Key);
        }

        [Fact]
        public void Delete()
        {
            // create
            BuildServer record = RecordHelper.CreateBuildServer(this.Postgres);

            // delete
            this.Postgres.DeleteBuildServer(record);

            // ensure delete succeeded
            BuildServer get = this.Postgres.GetBuildServerById(record.Key);
            Assert.Null(get);
        }

        [Fact]
        public void GetAll()
        {
            // create 
            BuildServer record1 = this.Postgres.SaveBuildServer(new BuildServer { Key = "buildserver1" });
            BuildServer record2 = this.Postgres.SaveBuildServer(new BuildServer { Key = "buildserver2" });


            IEnumerable<BuildServer> page = this.Postgres.GetBuildServers();
            Assert.Equal(2, page.Count());
            Assert.Contains(page, r => r.Key == record1.Key);
            Assert.Contains(page, r => r.Key == record2.Key);
        }

        #endregion
    }
}
