namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(PostProcessorPluginProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface IBuildEventHandler : IPlugin
    {
        void Process(Build build);
    }
}
