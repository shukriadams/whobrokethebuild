using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Xunit;

namespace Wbtb.Extensions.Data.Postgres.Tests
{
    [Collection("Sequential")]
    public class SessionTests : TestBase
    {
        #region TESTS

        [Fact]
        public void CreatePropertyCheck()
        {
            // create
            User user = RecordHelper.CreateUser(this.Postgres);
            Session record = this.Postgres.SaveSession(new Session
            {
                CreatedUtc = new DateTime(2001,1,1),
                IP = "127.0.0.1",
                UserAgent = "myagent",
                UserId = user.Key
            });

            Assert.NotEqual("0", record.Id);
            Assert.Equal("127.0.0.1", record.IP);
            Assert.Equal("myagent", record.UserAgent);
            Assert.Equal(user.Key, record.UserId);
            Assert.Equal(new DateTime(2001, 1, 1), record.CreatedUtc);
        }

        [Fact]
        public void Get()
        {
            // create
            Core.Common.Session record = RecordHelper.CreateSession(this.Postgres);

            // retrieve
            Core.Common.Session get = this.Postgres.GetSessionById(record.Id);
            Assert.NotNull(get);
        }

        [Fact]
        public void Update()
        {
            // create 
            Core.Common.Session record = RecordHelper.CreateSession(this.Postgres);

            // retrieve then update
            Core.Common.Session get = this.Postgres.GetSessionById(record.Id);
            get.IP = "new ip";
            this.Postgres.SaveSession(get);

            // retrieve again and ensure update succeeded
            get = this.Postgres.GetSessionById(record.Id);
            Assert.Equal("new ip", get.IP);
        }

        [Fact]
        public void Delete()
        {
            // create
            Core.Common.Session record = RecordHelper.CreateSession(this.Postgres);

            // delete
            this.Postgres.DeleteSession(record);

            // ensure delete succeeded
            Core.Common.Session get = this.Postgres.GetSessionById(record.Id);
            Assert.Null(get);
        }

        [Fact]
        public void GetByUser()
        {
            // create
            User user = RecordHelper.CreateUser(this.Postgres);
            Session record1 = RecordHelper.RandomSession();
            Session record2 = RecordHelper.RandomSession();
            Session record3 = RecordHelper.RandomSession();
            record1.UserId = user.Key;
            record2.UserId = user.Key;

            this.Postgres.SaveSession(record1);
            this.Postgres.SaveSession(record2);

            IEnumerable<Session> all = this.Postgres.GetSessionByUserId(user.Key);
            Assert.Equal(2, all.Count());
            Assert.Contains(all, r => r.Id == record1.Id);
            Assert.Contains(all, r => r.Id == record2.Id);
        }

        #endregion
    }
}
