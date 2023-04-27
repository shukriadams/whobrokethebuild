using System.Linq;
using Wbtb.Core.Common;
using Xunit;

namespace Wbtb.Extensions.Data.Postgres.Tests
{
    [Collection("Sequential")]
    public class UserTests : TestBase
    {
        #region TESTS

        [Fact]
        public void CreatePropertyCheck()
        {
            // create
            User record = this.Postgres.SaveUser(new User
            {
                Key = "my user"
            });

            Assert.NotEqual("0", record.Key);
            Assert.Equal("my user", record.Key);
        }

        [Fact]
        public void Get()
        {
            // create
            User record = RecordHelper.CreateUser(this.Postgres);

            // retrieve
            User get = this.Postgres.GetUserById(record.Key);
            Assert.NotNull(get);
        }

        [Fact]
        public void Update()
        {
            // create 
            User record = RecordHelper.CreateUser(this.Postgres);

            // retrieve then update
            User get = this.Postgres.GetUserById(record.Key);
            get.Key = "new name";
            this.Postgres.SaveUser(get);

            // retrieve again and ensure update succeeded
            get = this.Postgres.GetUserById(record.Key);
            Assert.Equal("new name", get.Key);
        }

        [Fact]
        public void Delete()
        {
            // create
            User record = RecordHelper.CreateUser(this.Postgres);

            // delete
            this.Postgres.DeleteUser(record);

            // ensure delete succeeded
            User get = this.Postgres.GetUserById(record.Key);
            Assert.Null(get);
        }

        [Fact]
        public void Page()
        {
            // create 
            User record1 = this.Postgres.SaveUser(new User { Key = "a" });
            User record2 = this.Postgres.SaveUser(new User { Key = "b" });
            User record3 = this.Postgres.SaveUser(new User { Key = "c" });

            PageableData<User> page = this.Postgres.PageUsers(0, 2);

            Assert.Equal(3, page.TotalItemCount);
            Assert.Equal(2, page.Items.Count());
            Assert.Contains(page.Items, r => r.Key == record1.Key);
            Assert.Contains(page.Items, r => r.Key == record2.Key);
        }

        #endregion
    }
}
