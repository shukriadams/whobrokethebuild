using Microsoft.AspNetCore.Mvc;

namespace Wbtb.Core.Web
{
    public class ErrorController : Controller
    {

        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [Route("error/404")]
        public IActionResult Error404()
        {
            this.HttpContext.Response.StatusCode = 404;
            return View();
        }

        [Route("error/500")]
        public IActionResult Error500()
        {
            this.HttpContext.Response.StatusCode = 500;
            return View();
        }

        [Route("error/notready")]
        public IActionResult NotReady()
        {
            this.HttpContext.Response.StatusCode = 503; // std not yet ready code
            return View();
        }

        [Route("error/configuration")]
        public IActionResult Configuration()
        {
            this.HttpContext.Response.StatusCode = 500;
            return View();
        }
    }
}
