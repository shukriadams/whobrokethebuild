﻿using System;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Extensions.Auth.ActiveDirectory;

namespace Wbtb.Extensions.Data.Postgres.Tests
{
    public abstract class TestBase
    {
        #region FIELDS

        protected IDataPlugin Postgres {get;set; }

        #endregion

        public TestBase()
        {
            SimpleDI di = new SimpleDI();
            di.Register<IDataPlugin,Postgres>();
            di.Register<IAuthenticationPlugin, ActiveDirectory>();

            throw new NotImplementedException("fix this");
            //Core.Core.LoadConfig(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.yml"));
            //Core.Core.LoadPlugins();

            Postgres = di.Resolve<IDataPlugin>();
            Configuration config = di.Resolve<Configuration>();
            Postgres.ContextPluginConfig = config.Plugins.First(p => p.Manifest.Concrete == TypeHelper.Name<Postgres>());
            PostgresCommon.ClearAllTables(Postgres.ContextPluginConfig);
        }
    }
}
