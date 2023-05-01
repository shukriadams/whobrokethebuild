using Ninject;
using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// For dev
    /// </summary>
    public class NinjectWrapper : IPluginFactory
    {
        public StandardKernel Kernel { private set; get; }

        public NinjectWrapper(StandardKernel kernel)
        {
            Kernel = kernel;
        }

        public object Get(Type t)
        { 
            return Kernel.Get(t);
        }
    }
}
