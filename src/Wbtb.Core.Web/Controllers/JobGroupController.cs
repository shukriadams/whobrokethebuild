using Microsoft.AspNetCore.Mvc;
using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class JobGroupController : Controller
    {
        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildid"></param>
        [ServiceFilter(typeof(ViewStatus))]
        [Route("{jobGroup}")]
        public IActionResult GetStatus(string jobGroup)
        {
            try
            {
                if (string.IsNullOrEmpty(jobGroup))
                    return new JsonResult(new
                    {
                        error = new
                        {
                            description = $"JobGroup required"
                        }
                    });

                SimpleDI di = new SimpleDI();
                JobGroupLogic jobGroupLogic = di.Resolve<JobGroupLogic>();
                JobGroupStatusResponse response = jobGroupLogic.GetStatus(jobGroup);

                if (!response.Success)
                    return new JsonResult(new
                    {
                        error = new
                        {
                            description = response.Message
                        }
                    });

                return new JsonResult(new
                {
                    success = new
                    {
                        status = response.Status.Value,
                        statusString = response.Status.Value.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new
                {
                    error = ex.ToString()
                });
            }
        }

        #endregion
    }
}
