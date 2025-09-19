namespace Wbtb.Core.Common
{
    public interface IPlugin
    {
        /// <summary>
        /// If the plugin needs to do work before it can run. Called on app start.
        /// </summary>
        /// <returns></returns>
        PluginInitResult InitializePlugin();

        /// <summary>
        /// Config for this instance of plugin
        /// </summary>
        PluginConfig ContextPluginConfig { get;set; }

        /// <summary>
        /// Can be left empty. Place diagnostic / debug features here. This method can be invoked directly in standalone mode.
        /// </summary>
        void Diagnose();
    }
}
