using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LogParseController : Controller
    {
        #region FIELDS

        private SimpleDI _di;

        #endregion

        #region CTORS

        public LogParseController() 
        {
            // Do not resolve types that depend on Configuration, they will fail to resolve if app is in error state
            _di = new SimpleDI();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Returns all log parse results for the given build
        /// </summary>
        /// <param name="buildid"></param>
        [ServiceFilter(typeof(ViewStatus))]
        [Route("build/{publicIdentifier}")]
        public IActionResult ByBuild(string publicIdentifier)
        {
            try
            {
                PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
                IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
                Build build = dataLayer.GetBuildByUniquePublicIdentifier(publicIdentifier);
                if (build == null)
                    return this.NotFound(new {
                        error = new {
                            description = $"Build {publicIdentifier} not found"
                        } 
                    });

                IEnumerable<BuildLogParseResult> results = dataLayer.GetBuildLogParseResultsByBuildId(build.Id);

                return new JsonResult(new
                {
                    success = new
                    {
                        LogParseResults = results
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
