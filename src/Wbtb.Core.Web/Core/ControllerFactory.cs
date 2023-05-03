using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Controllers;

namespace Wbtb.Core.Web
{
    public class ControllerFactory : IControllerFactory
    {
        SimpleDI _di;
        public ControllerFactory() 
        { 
            _di = new SimpleDI();
        }
        public object CreateController(ControllerContext context)
        {

            if (context.ActionDescriptor.ControllerName == "Home")
                return _di.Resolve<HomeController>();
            else if (context.ActionDescriptor.ControllerName == "Invoke")
                return _di.Resolve<InvokeController>();
            else if (context.ActionDescriptor.ControllerName == "Job")
                return _di.Resolve<JobController>();
            else if (context.ActionDescriptor.ControllerName == "Build")
                return _di.Resolve<BuildController>();

            throw new Exception($"Missing controller binding for route {context.ActionDescriptor.ControllerName}.");
        }

        public void ReleaseController(ControllerContext context, object controller)
        {
            // do nothing
        }
    }
}
