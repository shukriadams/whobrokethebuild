using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System;
using Wbtb.Core.Common;
using Microsoft.Extensions.Logging;

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

        [ServiceFilter(typeof(ViewStatus))]
        [Route("reset/{buildid}")]
        public void Reset(string buildid)
        {
            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            ILogger log = di.Resolve<ILogger>();
            dataLayer.TransactionStart();

            try
            {
                int deleted = dataLayer.ResetBuild(buildid, false);
                dataLayer.SaveDaemonTask(new DaemonTask
                {
                    BuildId = buildid,
                    Stage = 0, //"BuildEnd",
                    CreatedUtc = DateTime.UtcNow,
                    Src = this.GetType().Name
                });

                dataLayer.TransactionCommit();
                log.LogInformation($"Sucessfully reset build {buildid}");

            }
            catch (Exception ex)
            {
                dataLayer.TransactionCancel();
                log.LogError("Unexpected error", ex);
            }

            Response.Redirect("/processlog");
        }

        #endregion
    }
}
