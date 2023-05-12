namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(BuildLevelProcessorProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface IBuildLevelProcessor : IPlugin
    {
        void Process(Build build);
    }
}
