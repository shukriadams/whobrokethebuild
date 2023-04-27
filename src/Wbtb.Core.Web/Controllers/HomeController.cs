using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;
using Wbtb.Extensions.Auth.ActiveDirectory;
using Wbtb.Extensions.Messaging.Slack;
using Microsoft.AspNetCore.SignalR;
using System;
using Microsoft.Extensions.Logging;

namespace Wbtb.Core.Web.Controllers
{
    public class HomeController : Controller
    {
        #region FIELDS

        private IHubContext<ConsoleHub> _hub;

        private ILogger<HomeController> _log;

        #endregion

        #region CTORS

        public HomeController(IHubContext<ConsoleHub> hub, ILogger<HomeController> log)
        {
            _hub = hub;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [Route("")]
        public IActionResult Index()
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            // todo : replace with redirect to /buildserver/{id?}
            BuildServer buildServer = dataLayer.GetBuildServers().FirstOrDefault();
            IEnumerable<Job> jobs = new List<Job>();
            IList<ViewJob> viewjobs = new List<ViewJob>();

            JobsPageModel model = new JobsPageModel();

            if (buildServer != null)
                jobs = dataLayer.GetJobsByBuildServerId(buildServer.Id);

            foreach(Job job in jobs)
            {
                ViewJob v = ViewJob.Copy(job);
                viewjobs.Add(v);
                v.LatestBuild = dataLayer.GetLatestBuildByJob(job);
                v.BreakBuild = ViewBuild.Copy(dataLayer.GetBreakingBuildByJob(job));

                if (v.BreakBuild != null)
                    v.BreakBuild.BuildInvolvements = ViewBuildInvolvement.Copy(dataLayer.GetBuildInvolvementsByBuild(v.BreakBuild.Id));
            }

            model.AnyJobUsingBanners = viewjobs.Where(j => !string.IsNullOrEmpty(j.Image)).Any();
            model.Jobs = viewjobs;
            model.Title = "my jobs";

            return View(model);
        }


        [Route("/buildprocessorlog/{buildprocessorid}")]
        public IActionResult BuildProcessorLog(string buildProcessorId)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();

            BuildProcessorLogPageModel model = new BuildProcessorLogPageModel();
            BuildProcessor buildProcessor = dataLayer.GetBuildProcessorById(buildProcessorId);
            if (buildProcessor == null)
                return Responses.NotFoundError($"BuildProcessor {buildProcessorId} does not exist");

            model.Build = dataLayer.GetBuildById(buildProcessor.BuildId);
            model.Log = LogHelper.GetBuildProcessorLog(buildProcessor.BuildId, buildProcessor.Id);
            model.BuildProcessor = buildProcessor;

            return View(model);
        }


        [Route("/admin")]
        public IActionResult Admin()
        {
            LayoutModel model = new LayoutModel(); 
            return View(model);
        }


        [Route("/jobsoftreset/{jobId}")]
        public IActionResult SoftResetJob(string jobId)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            int deleted = dataLayer.ResetJob(jobId, false);
            System.Console.WriteLine($"Reset {deleted} incident from job {jobId}");

            return Redirect($"/job/{jobId}");
        }

        [Route("/buildsoftreset/{buildId}")]
        public IActionResult SoftResetBuild(string buildId) 
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            int deleted = dataLayer.ResetBuild(buildId, false);
            System.Console.WriteLine($"Reset {deleted} incident from build {buildId}");

            return Redirect($"/build/{buildId}");
        }

        [Route("/jobhardreset/{jobId}")]
        public IActionResult HardResetJob(string jobId)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            int deleted = dataLayer.ResetJob(jobId, true);
            System.Console.WriteLine($"Reset {deleted} incident from job {jobId}");

            return Redirect($"/job/{jobId}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("/console")]
        public IActionResult Console()
        {
            LayoutModel model = new LayoutModel();
            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("/test")]
        public IActionResult Test()
        {
            _hub.Clients.All.SendAsync("ReceiveMessage", "some user", "some message");
            LayoutModel model = new LayoutModel();
            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobid"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [Route("/job/{jobid}/{pageIndex?}")]
        public IActionResult Job(string jobid, int pageIndex)
        {
            // force start-at-zero if value set, pager will never use 0
            if (pageIndex > 0)
                pageIndex--;

            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            JobPageModel model = new JobPageModel();
            model.Job = ViewJob.Copy(dataLayer.GetJobById(jobid));
            if (model.Job == null)
                return Responses.NotFoundError($"Job {jobid} does not exist");

            model.Job.LatestBuild = dataLayer.GetLatestBuildByJob(model.Job);
            model.Job.BreakBuild = ViewBuild.Copy(dataLayer.GetBreakingBuildByJob(model.Job));

            model.Stats = dataLayer.GetJobStats(model.Job);
            model.BaseUrl = $"/job/{jobid}";
            model.Builds = ViewBuild.Copy(dataLayer.PageBuildsByJob(jobid, pageIndex, ConfigKeeper.Instance.StandardPageSize));
            return View(model);
        }
        
        [Route("/incidents/{jobid}/{pageIndex?}")]
        public IActionResult JobIncidents(string jobid, int pageIndex)
        {
            // force start-at-zero if value set, pager will never use 0
            if (pageIndex > 0)
                pageIndex--;

            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            JobIncidentsModel model = new JobIncidentsModel();
            model.BaseUrl = $"/job/incidents/{jobid}";
            model.Builds = ViewBuild.Copy(dataLayer.PageIncidentsByJob(jobid, pageIndex, ConfigKeeper.Instance.StandardPageSize));
            return View(model);
        }

        [Route("/buildflag/reset/{buildid}/{flag}")]
        public IActionResult BuildFlagReset(string buildid, string flag)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            Build build = dataLayer.GetBuildById(buildid);
            int affected = dataLayer.IgnoreBuildFlagsForBuild(build, (BuildFlags)Enum.Parse(typeof(BuildFlags), flag));
            return Redirect($"/build/{buildid}");
        }

        [Route("/buildflag/delete/{buildflagid}")]
        public string BuildFlagDelete(string buildflagid)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            BuildFlag buildFlag = dataLayer.GetBuildFlagById(buildflagid);
            dataLayer.DeleteBuildFlag(buildFlag);
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildid"></param>
        /// <returns></returns>
        [Route("/build/{buildid}")]
        public IActionResult Build(string buildid)
        {
            BuildPageModel model = new BuildPageModel();
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            model.Build = ViewBuild.Copy(dataLayer.GetBuildById(buildid));
            if (model.Build == null)
                return Responses.NotFoundError($"build {buildid} does not exist");

            model.Build.Job = ViewJob.Copy(dataLayer.GetJobById(model.Build.JobId));

            BuildServer buildServer = dataLayer.GetBuildServerById(model.Build.Job.BuildServerId);
            IBuildServerPlugin buildServerPlugin = PluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;

            model.BuildInvolvements = ViewBuildInvolvement.Copy(dataLayer.GetBuildInvolvementsByBuild(buildid));
            foreach (ViewBuildInvolvement bi in model.BuildInvolvements)
                if (bi.RevisionId != null)
                    bi.Revision = dataLayer.GetRevisionById(bi.RevisionId);
            
            model.PreviousBuild = dataLayer.GetPreviousBuild(model.Build);
            model.NextBuild = dataLayer.GetNextBuild(model.Build);
            model.LinkToBuildSystem = buildServerPlugin.GetBuildUrl(buildServer, model.Build);
            model.BuildParseResults = dataLayer.GetBuildLogParseResultsByBuildId(buildid);
            model.Build.IncidentBuild = string.IsNullOrEmpty(model.Build.IncidentBuildId) ? null : ViewBuild.Copy(dataLayer.GetBuildById(model.Build.IncidentBuildId));
            model.BuildFlags = dataLayer.GetBuildFlagsForBuild(model.Build);
            model.RevisionsLinkedFromLog = !string.IsNullOrEmpty(model.Build.Job.RevisionAtBuildRegex);
            model.buildProcessors = dataLayer.GetBuildProcessorsByBuildId(model.Build.Id);

            return View(model);
        }

        [Route("/build/log/{buildid}")]
        public IActionResult BuildLog(string buildid)
        {
            BuildLogParseResultsPageModel model = new BuildLogParseResultsPageModel();
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            model.Build = ViewBuild.Copy(dataLayer.GetBuildById(buildid));
            if (model.Build == null)
                return Responses.NotFoundError($"build {buildid} does not exist");

            if (model.Build.LogPath != null)
            { 
                if (System.IO.File.Exists(model.Build.LogPath))
                    model.Raw = System.IO.File.ReadAllText(model.Build.LogPath);
                else
                    return Responses.UnknownError($"The log for build {buildid} was not found at expected path {model.Build.LogPath}", 120); // todo : proper code needed
            }

            return View(model);
        }

        [Route("/buildhost/{hostname}/{page?}")]
        public IActionResult BuildHost(string hostname, int page)
        {
            hostname = HttpUtility.UrlDecode(hostname);
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            BuildHostModel model = new BuildHostModel();

            model.Hostname = hostname;
            model.Builds = ViewBuild.Copy(dataLayer.PageBuildsByBuildAgent(hostname, page > 0 ? page - 1 : page, ConfigKeeper.Instance.StandardPageSize));
            model.Builds.Items.ToList().ForEach(build => build.Job = ViewJob.Copy(dataLayer.GetJobById(build.JobId)));
            model.BaseUrl = $"/buildhost/{hostname}"; 

            return View(model);
        }

        [Route("/user/{userid}")]
        public new IActionResult User(string userid)
        {
            UserPageModel model = new UserPageModel();
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            model.User = dataLayer.GetUserById(userid);
            
            if (model.User == null)
            { 
                model.Username = userid;
            } 
            else 
            { 
                model.RecentBreaking = ViewBuildInvolvement.Copy(dataLayer.PageBuildInvolvementsByUserAndStatus(model.User.Id, BuildStatus.Failed, 0, 10).Items);
                model.RecentPassing= ViewBuildInvolvement.Copy(dataLayer.PageBuildInvolvementsByUserAndStatus(model.User.Id, BuildStatus.Passed, 0, 10).Items);

                foreach (ViewBuildInvolvement bi in model.RecentBreaking)
                    bi.Build = ViewBuild.Copy(dataLayer.GetBuildById(bi.BuildId));

                foreach (ViewBuildInvolvement bi in model.RecentPassing)
                    bi.Build = ViewBuild.Copy(dataLayer.GetBuildById(bi.BuildId));

            }

            return View(model);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("/users")]
        public IActionResult Users()
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            UsersPageModel model = new UsersPageModel();
            model.Users = dataLayer.GetUsers();
            return View(model);
        }


        [Route("/ad")]
        public IActionResult Ad()
        {
            IAuthenticationPlugin authentication = PluginProvider.GetFirstForInterface<IAuthenticationPlugin>();
            ActiveDirectory ad = authentication as ActiveDirectory;
            ad.ListUsers();
            ViewData["description"] = "done";

            LayoutModel model = new LayoutModel();
            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("/buildservers")]
        public IActionResult BuildServers()
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            IEnumerable<BuildServer> buildServers = dataLayer.GetBuildServers();
            ViewData["buildservers"] = buildServers;
            LayoutModel model = new LayoutModel();
            return View(model);
        }

        /// <summary>
        /// Lists remove jobs on build server, NOT jobs in local db
        /// </summary>
        /// <param name="buildserverid"></param>
        /// <returns></returns>
        [Route("/jobs/{buildserverid}")]
        public IActionResult Jobs(string buildserverid)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            BuildServer buildServer = dataLayer.GetBuildServerByKey(buildserverid);

            if (buildServer == null)
                return Redirect("buildserver not found");

            IBuildServerPlugin buildServerPlugin = PluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
            IEnumerable<string> jobs = buildServerPlugin.ListRemoteJobsCanonical(buildServer);

            ViewData["jobs"] = jobs;
            ViewData["buildServer"] = buildServer;

            LayoutModel model = new LayoutModel();
            return View(model);
        }

        /// <summary>
        /// DEV ONLY!
        /// </summary>
        /// <returns></returns>
        [Route("/perforce")]
        public IActionResult Perforce()
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            SourceServer sourceServer = dataLayer.GetSourceServers().First();
            //Revision revision = _sourceServer.GetRevision(sourceServer, "51112");
            //ViewData["Description"] = revision.Description;

            LayoutModel model = new LayoutModel();
            return View(model);
        }


        [Route("/buildflags/{page?}")]
        public IActionResult BuildFlags(int page)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            // force start-at-zero if value set, pager will never use 0
            if (page > 0)
                page--;

            BuildFlagsModel model = new BuildFlagsModel();
            model.BaseUrl = $"/buildflags";
            model.BuildFlags = ViewBuildFlag.Copy(dataLayer.PageBuildFlags(page, ConfigKeeper.Instance.StandardPageSize));
            return View(model);

        }

        [Route("/slack")]
        public IActionResult Slack()
        {
            Slack plugin = PluginProvider.GetFirstForInterface<IMessaging>() as Slack;

            //string result = plugin.TestHandler(ConfigKeeper.Instance.BuildServers.First().Jobs.First().Alert.First());
            plugin.ListChannels();
            ViewData["Description"] = "see console";

            LayoutModel model = new LayoutModel();
            return View(model);
        }

        [Route("/build/reprocesslog/{buildid}")]
        public string ReprocessLog(string buildid)
        {
            IDataLayerPlugin dataLayer = PluginProvider.GetFirstForInterface<IDataLayerPlugin>();
            Build build = dataLayer.GetBuildById(buildid);
            Job job = dataLayer.GetJobById(build.JobId);
            List<ILogParser> logParsers = new List<ILogParser>();
            foreach (string logParserName in job.LogParserPlugins)
                logParsers.Add(PluginProvider.GetByKey(logParserName) as ILogParser);

            // delete existing log processes
            foreach(BuildLogParseResult logParseResult in dataLayer.GetBuildLogParseResultsByBuildId(buildid))
                dataLayer.DeleteBuildLogParseResult(logParseResult);

            foreach (ILogParser logParser in logParsers)
                BuildLogParseResultHelper.ProcessBuild(dataLayer, build, logParser, _log);

            return string.Empty;
        }

        #endregion
    }
}
