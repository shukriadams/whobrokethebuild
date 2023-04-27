using Microsoft.AspNetCore.Mvc;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Web
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BuildController : Controller
    {
        /// <summary>
        /// TEMPORARY, REMOVE THIS
        /// </summary>
        /// <param name="buildid"></param>
        /// <returns></returns>
        [HttpDelete("{buildid}")]
        public string BuildDelete(string buildid)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            Build build = dataLayer.GetBuildById(buildid);
            if (build != null)
                dataLayer.DeleteBuild(build);

            return "deleted";
        }
    }
}
