using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Apply this to all endpoints that return an html view. See APIStatusFilter for other endpoints.
    /// </summary>
    public class ViewStatus : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // do nothing
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (AppState.ConfigErrors /*&& (string)context.HttpContext.Request.Path != "/ConfigErrors"*/)
            {
                context.Result = new ViewResult
                {
                    ViewName = "~/Views/Error/Configuration.cshtml"
                };
            }
            else if (!AppState.Ready /*&& (string)context.HttpContext.Request.Path == "/NotReady"*/)
            {
                context.Result = new ViewResult
                {
                    ViewName = "~/Views/Error/NotReady.cshtml"
                };
            }
        }
    }
}
