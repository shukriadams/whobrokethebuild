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
            LowEffortDI di = new LowEffortDI();
            return di.Resolve(t);
        }
    }
}
