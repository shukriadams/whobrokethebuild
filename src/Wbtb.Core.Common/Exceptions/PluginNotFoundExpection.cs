using System;

namespace Wbtb.Core.Common
{
    public class PluginNotFoundExpection : Exception
    {
        public PluginNotFoundExpection(Type plugin) : base ($"Expected plugin for type {plugin} not found") 
        { 

        }
    }
}
