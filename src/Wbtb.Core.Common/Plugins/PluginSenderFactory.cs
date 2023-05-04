using System;

namespace Wbtb.Core.Common.Plugins
{
    public class PluginSenderFactory : ISimpleDIFactory
    {
        #region FIELDS

        private readonly ConfigBasic _configBasic;

        private readonly Config _config;

        private readonly SimpleDI _di;
        #endregion

        #region CTORS

        public PluginSenderFactory(Config config, ConfigBasic configBasic) 
        {
            _config = config;
            _configBasic = configBasic;
            _di = new SimpleDI();
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
                return _di.Resolve<PluginCoreSender>();

            if (_configBasic.ProxyMode == "direct")
                return _di.Resolve<PluginDirectSender>();

            return _di.Resolve<PluginShellSender>();
        }

        #endregion
    }
}
