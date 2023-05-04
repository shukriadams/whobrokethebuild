namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(BuildLevelProcessorProxy))]
    [PluginBehaviour(false)]
    public interface IBuildLevelProcessor : IPlugin
    {
        void Process(Build build);
    }
}
