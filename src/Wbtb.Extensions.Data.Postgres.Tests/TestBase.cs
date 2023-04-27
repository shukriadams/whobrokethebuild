using Ninject;
using System;
using System.IO;
using System.Linq;
using Wbtb.Core;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;
using Wbtb.Extensions.Auth.ActiveDirectory;

namespace Wbtb.Extensions.Data.Postgres.Tests
{
    public abstract class TestBase
    {
        #region FIELDS

        protected Postgres Postgres {get;set; }

        #endregion

        public TestBase()
        {
            StandardKernel kernel = new StandardKernel();
            kernel.Bind<IDataLayerPlugin>().To<Postgres>();
            kernel.Bind<IAuthenticationPlugin>().To<ActiveDirectory>();
            PluginProvider.Factory = new NinjectWrapper(kernel);

            throw new NotImplementedException("fix this");
            //Core.Core.LoadConfig(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.yml"));
            //Core.Core.LoadPlugins();

            Postgres = new Postgres();
            Postgres.ContextPluginConfig = ConfigKeeper.Instance.Plugins.First(p => p.Manifest.Concrete == TypeHelper.Name<Postgres>());
            PostgresCommon.ClearAllTables(Postgres.ContextPluginConfig);
        }
    }
}
