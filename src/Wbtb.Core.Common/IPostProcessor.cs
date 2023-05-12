namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(BuildLevelProcessorProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface IPostProcessor : IPlugin
    {
        PostProcessResult Process();
    }
}
