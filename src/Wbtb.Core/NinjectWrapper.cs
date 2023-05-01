using Ninject;
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
            using (IKernel kernel = new StandardKernel())    
                return kernel.Get(t);
        }
    }
}
