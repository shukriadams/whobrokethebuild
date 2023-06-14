using Microsoft.AspNetCore.Mvc;

namespace Wbtb.Core.Web
{
    public class FallbackController : Controller
    {

        [Route("/notready")]
        public IActionResult NotReady()
        {
            return View();
        }

        [Route("/configErrors")]
        public IActionResult ConfigErrors()
        {
            return View();
        }
    }
}
