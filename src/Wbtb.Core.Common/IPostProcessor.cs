namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(BuildLevelProcessorProxy))]
    [PluginBehaviour(false)]
    public interface IPostProcessor : IPlugin
    {
        PostProcessResult Process();
    }
}
