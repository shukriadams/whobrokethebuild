namespace Wbtb.Core.Common
{
    public interface IPluginProvider
    {
        T GetFirstForInterface<T>(bool expected = true);
        IPlugin GetByKey(string key);
        TPluginType GetDistinct<TPluginType>();
        object GetDistinct(PluginConfig pluginConfig, bool forceConcrete = true);
    }    
}

