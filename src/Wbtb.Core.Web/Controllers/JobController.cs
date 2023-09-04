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
                            Stage = 0, //"BuildEnd",
                            CreatedUtc = DateTime.UtcNow,
                            Src = this.GetType().Name
                        });

                        Thread.Sleep(10);
                        Console.WriteLine($"Requeued build {build.Identifier} for processing.");
                    }

                    page++;
                }

                dataLayer.TransactionCommit();
                log.LogInformation($"Sucessfully reset job {jobid}");

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
