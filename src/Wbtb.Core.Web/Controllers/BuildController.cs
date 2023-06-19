using Microsoft.AspNetCore.Mvc;
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
        
        #region FIELDS

        public BuildController() 
        {
            _di = new SimpleDI();
            _pluginProvider = _di.Resolve<PluginProvider>();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// TEMPORARY, REMOVE THIS
        /// </summary>
        /// <param name="buildid"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ViewStatus))]
        [HttpDelete("{buildid}")]
        public string BuildDelete(string buildid)
        {
            IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
            Build build = dataLayer.GetBuildById(buildid);
            if (build != null)
                dataLayer.DeleteBuild(build);

            return "deleted";
        }

        #endregion
    }
}
