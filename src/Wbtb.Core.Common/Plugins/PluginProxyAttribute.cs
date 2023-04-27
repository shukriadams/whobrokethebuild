using System;

namespace Wbtb.Core.Common.Plugins
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public class PluginProxyAttribute : Attribute
    {
        public Type ProxyType { get; private set; }

        public PluginProxyAttribute(Type proxy)
        {
            this.ProxyType = proxy;
        }

    }
}
