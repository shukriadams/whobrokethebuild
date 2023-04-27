using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Xunit;

namespace Wbtb.Extensions.Data.Postgres.Tests
{
    [Collection("Sequential")]
    public class SourceServerTests : TestBase
    {
        #region TESTS

        [Fact]
        public void CreatePropertyCheck()
        {
            // create
            SourceServer record = this.Postgres.SaveSourceServer(new SourceServer
            {
                Key = "my source server"
            });

            Assert.NotEqual("0", record.Key);
            Assert.Equal("my source server", record.Key);
        }

        [Fact]
        public void Get()
        {
            // create
            SourceServer record = RecordHelper.CreateSourceServer(this.Postgres);

            // retrieve
            SourceServer get = this.Postgres.GetSourceServerById(record.Key);
            Assert.NotNull(get);
        }


        [Fact]
        public void Update()
        {
            // create 
            SourceServer record = RecordHelper.CreateSourceServer(this.Postgres);

            // retrieve then update
            SourceServer get = this.Postgres.GetSourceServerById(record.Key);
            get.Key = "new name";
            this.Postgres.SaveSourceServer(get);

            // retrieve again and ensure update succeeded
            get = this.Postgres.GetSourceServerById(record.Key);
            Assert.Equal("new name", get.Key);
        }

        [Fact]
        public void Delete()
        {
            // create
            SourceServer record = RecordHelper.CreateSourceServer(this.Postgres);

            // delete
            this.Postgres.DeleteSourceServer(record);

            // ensure delete succeeded
            SourceServer get = this.Postgres.GetSourceServerById(record.Key);
            Assert.Null(get);
        }

        [Fact]
        public void GetAll()
        {
            // create 
            SourceServer record1 = this.Postgres.SaveSourceServer(new SourceServer { Key = "sourceserver1" });
            SourceServer record2 = this.Postgres.SaveSourceServer(new SourceServer { Key = "sourceserver2" });


            IEnumerable<SourceServer> page = this.Postgres.GetSourceServers();
            Assert.Equal(2, page.Count());
            Assert.Contains(page, r => r.Key == record1.Key);
            Assert.Contains(page, r => r.Key == record2.Key);
        }

        #endregion
    }
}
