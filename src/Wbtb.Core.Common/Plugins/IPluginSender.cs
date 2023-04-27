using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    public interface IPluginSender
    {
        /// <summary>
        /// handshakes plugin at app start - determines if plugin is available, and if config in app meets plugin's requirements
        /// </summary>
        /// <param name="pluginName"></param>
        /// <returns></returns>
        PluginInitResult Initialize(string pluginName);

        /// <summary>
        /// Invokes a method in plugin with a return value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingProxy"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        T InvokeMethod<T>(IPluginProxy callingProxy, PluginArgs args);

        /// <summary>
        /// Invokes a method in plugin with no return value.
        /// </summary>
        /// <param name="callingProxy"></param>
        /// <param name="args"></param>
        void InvokeMethod(IPluginProxy callingProxy, PluginArgs args);
    }
}
