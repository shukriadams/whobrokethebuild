namespace Wbtb.Core.Common
{
    public interface IPluginSender
    {
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
