using System;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    /// <summary>
    /// For dev
    /// </summary>
    public class NinjectWrapper : IPluginFactory
    {
        public object Get(Type t)
        {
            SimpleDI di = new SimpleDI();
            return di.Resolve(t);
        }
    }
}
