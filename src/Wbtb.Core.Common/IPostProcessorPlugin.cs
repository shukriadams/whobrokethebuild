namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(PostProcessorPluginProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface IPostProcessorPlugin : IPlugin
    {
        PostProcessResult Process(Build build);
    }
}
