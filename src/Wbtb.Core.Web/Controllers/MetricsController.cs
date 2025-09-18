using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Core;

namespace Wbtb.Core.Web
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MetricsController : Controller
    {
        #region METHODS

        [ServiceFilter(typeof(ViewStatus))]
        [Route("influx")]
        public ActionResult<string> Influx()
        {
            SimpleDI di = new SimpleDI();
            ILogger log = di.Resolve<ILogger>();

            try
            {
                MetricsHelper metricsHelper = di.Resolve<MetricsHelper>(); 
                return metricsHelper.GetInflux();
            }
            catch (Exception ex)
            {
                log.LogError("unexpected error serving influx metrics", ex);
                return "ERROR, check logs";
            }
        }

        #endregion
    }
}
