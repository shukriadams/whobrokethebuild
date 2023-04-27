namespace Wbtb.Core.Common
{
    public interface IPluginProxy
    {
        /// <summary>
        /// Id of the plugin being proxied.
        /// phase out, use pluginconfig.id that is always present
        /// </summary>
        string PluginKey { get;set;}
    }
}
