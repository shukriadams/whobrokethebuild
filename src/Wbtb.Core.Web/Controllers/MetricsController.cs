using Microsoft.AspNetCore.Mvc;
using System;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Core;

namespace Wbtb.Core.Web
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MetricsController : Controller
    {
        public Logger Logger { get; set; }

        #region CTORS

        public MetricsController()
        {
            SimpleDI di = new SimpleDI();
            Logger = di.Resolve<Logger>();
        }

        #endregion

        #region METHODS

        [ServiceFilter(typeof(ViewStatus))]
        [Route("influx")]
        public ActionResult<string> Influx()
        {
            SimpleDI di = new SimpleDI();

            try
            {
                MetricsHelper metricsHelper = di.Resolve<MetricsHelper>(); 
                return metricsHelper.GetInflux();
            }
            catch (Exception ex)
            {
                Logger.Error(this, "unexpected error serving influx metrics", ex);
                return "ERROR, check logs";
            }
        }

        #endregion
    }
}
