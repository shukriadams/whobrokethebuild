using System;

namespace Wbtb.Core.Common
{
    // shim for ninject so it doesn't have to live in common
    public interface IPluginFactory
    {
        object Get(Type t);
    }
}
