using System;

namespace Wbtb.Core.Common.Plugins
{
    public class PluginSenderFactory : ISimpleDIFactory
    {
        #region FIELDS

        private readonly ConfigBasic _configBasic;

        private readonly Config _config;

        #endregion

        #region CTORS

        public PluginSenderFactory(Config config, ConfigBasic configBasic) 
        {
            _config = config;
            _configBasic = configBasic;
        }

        #endregion

        #region METHODS

        public object Resolve<T>()
        {
            return this.Resolve(typeof(T));
        }

        public object Resolve(Type service)
        {
            if (_config.IsCurrentContextProxyPlugin)
                return new PluginCoreSender();

            if (_configBasic.ProxyMode == "direct")
                return new PluginDirectSender();

            SimpleDI di = new SimpleDI();
            return di.Resolve<PluginShellSender>();
        }

        #endregion
    }
}
