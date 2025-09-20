using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class JobController : Controller
    {
        private readonly Logger _logger;

        public JobController() 
        {
            SimpleDI dI = new SimpleDI();
            _logger = dI.Resolve<Logger>();
        }

        [ServiceFilter(typeof(ViewStatus))]
        [Route("reset/{jobid}")]
        public void Reset(string jobid)
        {
            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            ILogger log = di.Resolve<ILogger>();
            dataLayer.TransactionStart();

            try 
            {
                // reset job
                int deleted = dataLayer.ResetJob(jobid, false);
                int page = 0;

                // reset all jobs
                while (true)
                {
                    PageableData<Build> builds = dataLayer.PageBuildsByJob(jobid, page, 100, true);
                    if (builds.Items.Count == 0)
                        break;

                    foreach (Build build in builds.Items)
                    {
                        
                        dataLayer.SaveDaemonTask(new DaemonTask
                        {
                            BuildId = build.Id,
                            Stage = (int)ProcessStages.BuildEnd, 
                            CreatedUtc = DateTime.UtcNow,
                            Src = this.GetType().Name
                        });

                        Thread.Sleep(10);
                        _logger.Status($"Requeued build {build.Key} for processing.");
                    }

                    page++;
                }

                dataLayer.TransactionCommit();
                log.LogInformation($"Successfully reset job {jobid}");

            }
            catch (Exception ex) 
            {
                dataLayer.TransactionCancel();
                log.LogError("Unexpected error", ex);
            }

            Response.Redirect("/processlog");
        }
    }
}
