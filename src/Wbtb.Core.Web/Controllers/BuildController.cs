using Microsoft.AspNetCore.Mvc;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Web
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BuildController : Controller
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        #endregion
        
        #region FIELDS

        public BuildController() 
        { 
            SimpleDI di = new SimpleDI();
            _pluginProvider = di.Resolve<PluginProvider>();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// TEMPORARY, REMOVE THIS
        /// </summary>
        /// <param name="buildid"></param>
        /// <returns></returns>
        [HttpDelete("{buildid}")]
        public string BuildDelete(string buildid)
        {
            IDataLayerPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            Build build = dataLayer.GetBuildById(buildid);
            if (build != null)
                dataLayer.DeleteBuild(build);

            return "deleted";
        }

        #endregion
    }
}
