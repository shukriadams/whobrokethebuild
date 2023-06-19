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
            return View();
        }

        [Route("error/500")]
        public IActionResult Error500()
        {
            return View();
        }

        [Route("error/notready")]
        public IActionResult NotReady()
        {
            return View();
        }

        [Route("error/config")]
        public IActionResult Config()
        {
            return View();
        }
    }
}
