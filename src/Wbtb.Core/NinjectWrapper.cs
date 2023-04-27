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
        private StandardKernel _kernel;

        public NinjectWrapper(StandardKernel kernel)
        {
            _kernel = kernel;
        }

        public object Get(Type t)
        { 
            return _kernel.Get(t);
        }
    }
}
