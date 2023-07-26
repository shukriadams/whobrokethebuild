namespace Wbtb.Core.Common
{
    [PluginProxy(typeof(PostProcessorPluginProxy))]
    [PluginBehaviour(allowMultiple: true)]
    public interface IPostProcessorPlugin : IPlugin
    {
        void VerifyJobConfig(Job job);

        PostProcessResult Process(Build build);
    }
}
