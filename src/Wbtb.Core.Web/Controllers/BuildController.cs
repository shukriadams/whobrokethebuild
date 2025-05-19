using Microsoft.AspNetCore.Mvc;
using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BuildController : Controller
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        #endregion
        
        #region CTORS

        public BuildController() 
        {
            _di = new SimpleDI();
            _pluginProvider = _di.Resolve<PluginProvider>();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Returns page of build ids for a given job, sorted by descending s
        /// </summary>
        /// <param name="buildid"></param>
        [ServiceFilter(typeof(ViewStatus))]
        [Route("job/{jobKey}")]
        public IActionResult ByJob(string jobKey, int? index)
        {
            int pageSize = 100;

            try
            {
                if (!index.HasValue)
                    index = 0;

                PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
                IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
                Job job = dataLayer.GetJobByKey(jobKey);
                if (job == null)
                    return this.NotFound(new
                    {
                        error = new
                        {
                            description = $"Job {jobKey} not found"
                        }
                    });

                PageableData<Build> results = dataLayer.PageBuildsByJob(job.Id, index.Value, pageSize, false);

                return new JsonResult(new
                {
                    success = new
                    {
                        builds = results
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
