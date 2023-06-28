using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web.Controllers
{
    public class HomeController : Controller
    {
        #region FIELDS

        private ILogger _log;

        private SimpleDI _di;

        #endregion

        #region CTORS

        /// <summary>
        /// M$ hides how to pass HTTP context to custom controller factory, so we need to do all DI inside the CTOR.
        /// </summary>
        public HomeController()
        {
            // Do not resolve types that depend on Configuration, they will fail to resolve if app is in error state
            _di = new SimpleDI();
            _log = _di.Resolve<ILogger>();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Renders view.
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ViewStatus))]
        [Route("")]
        public IActionResult Index()
        {
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();

            // todo : replace with redirect to /buildserver/{id?}
            BuildServer buildServer = dataLayer.GetBuildServers().FirstOrDefault();
            IEnumerable<Job> jobs = new List<Job>();
            IList<ViewJob> tempjobs = new List<ViewJob>();
            JobsPageModel model = new JobsPageModel();

            if (buildServer != null)
                jobs = dataLayer.GetJobsByBuildServerId(buildServer.Id);

            foreach(Job job in jobs)
            {
                ViewJob v = ViewJob.Copy(job);
                tempjobs.Add(v);
                v.LatestBuild = dataLayer.GetLatestBuildByJob(job);
                v.DeltaBuild = ViewBuild.Copy(dataLayer.GetLastJobDelta(job.Id));

                if (v.DeltaBuild != null)
                    v.DeltaBuild.BuildInvolvements = ViewBuildInvolvement.Copy(dataLayer.GetBuildInvolvementsByBuild(v.DeltaBuild.Id));
            }

            IList<ViewJob> viewjobs = new List<ViewJob>();
            viewjobs = viewjobs.Concat(tempjobs.Where(j => j.LatestBuild != null && j.LatestBuild.Status == BuildStatus.Failed)).ToList();
            viewjobs = viewjobs.Concat(tempjobs.Where(j => j.LatestBuild != null && j.LatestBuild.Status == BuildStatus.InProgress)).ToList();
            viewjobs = viewjobs.Concat(tempjobs.Where(j => j.LatestBuild != null && j.LatestBuild.Status == BuildStatus.Passed)).ToList();
            viewjobs = viewjobs.Concat(tempjobs.Where(j => j.LatestBuild == null || (j.LatestBuild.Status != BuildStatus.Failed && j.LatestBuild.Status != BuildStatus.InProgress && j.LatestBuild.Status != BuildStatus.Passed))).ToList();

            model.Jobs = viewjobs;
            model.Title = "my jobs";

            return View(model);
        }

        [ServiceFilter(typeof(ViewStatus))]
        [Route("/incident/{incidentId}")]
        public IActionResult Incident(string incidentId)
        {
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();

            IncidentPageModel model = new IncidentPageModel();
            model.IncidentBuild = dataLayer.GetBuildById(incidentId);
            if (model.IncidentBuild == null)
                return Responses.NotFoundError($"build {incidentId} does not exist");

            Build jobDelta = dataLayer.GetLastJobDelta(model.IncidentBuild.JobId);
            if (jobDelta != null && jobDelta.IncidentBuildId == incidentId)
                model.IsActive = true;

            model.Job = dataLayer.GetJobById(model.IncidentBuild.JobId);
            model.InvolvedBuilds = dataLayer.GetBuildsByIncident(incidentId);
            return View(model);
        }

        [ServiceFilter(typeof(ViewStatus))]
        [Route("/login")]
        public IActionResult Login()
        {
            LayoutModel model = new LayoutModel();
            return View(model);
        }

        [ServiceFilter(typeof(ViewStatus))]
        [Route("/buildsoftreset/{buildId}")]
        public IActionResult SoftResetBuild(string buildId) 
        {
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            int deleted = dataLayer.ResetBuild(buildId, false);
            System.Console.WriteLine($"Reset {deleted} incident from build {buildId}");

            return Redirect($"/build/{buildId}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(ViewStatus))]
        [Route("/console")]
        public IActionResult Console()
        {
            LayoutModel model = new LayoutModel();
            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobid"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ViewStatus))]
        [Route("/job/{jobid}/{pageIndex?}")]
        public IActionResult Job(string jobid, int pageIndex)
        {
            Configuration config = _di.Resolve<Configuration>();
            // force start-at-zero if value set, pager will never use 0
            if (pageIndex > 0)
                pageIndex--;

            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            JobPageModel model = new JobPageModel();
            model.Job = ViewJob.Copy(dataLayer.GetJobById(jobid));
            if (model.Job == null)
                return Responses.NotFoundError($"Job {jobid} does not exist");

            model.Job.LatestBuild = dataLayer.GetLatestBuildByJob(model.Job);
            model.Job.DeltaBuild = ViewBuild.Copy(dataLayer.GetLastJobDelta(model.Job.Id));
            model.Stats = dataLayer.GetJobStats(model.Job);
            model.BaseUrl = $"/job/{jobid}";
            model.Builds = ViewBuild.Copy(dataLayer.PageBuildsByJob(jobid, pageIndex, config.StandardPageSize, false));
            return View(model);
        }

        [ServiceFilter(typeof(ViewStatus))]
        [Route("/incidents/{jobid}/{pageIndex?}")]
        public IActionResult JobIncidents(string jobid, int pageIndex)
        {
            // force start-at-zero if value set, pager will never use 0
            if (pageIndex > 0)
                pageIndex--;

            Configuration config = _di.Resolve<Configuration>();
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            JobIncidentsModel model = new JobIncidentsModel();
            model.BaseUrl = $"/job/incidents/{jobid}";
            model.Job = ViewJob.Copy(dataLayer.GetJobById(jobid));
            PageableData<Build> incidentBuilds = dataLayer.PageIncidentsByJob(jobid, pageIndex, config.StandardPageSize);
            model.Builds = ViewIncidentCauseBuild.Copy(incidentBuilds);

            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildid"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ViewStatus))]
        [Route("/build/{buildid}")]
        public IActionResult Build(string buildid)
        {
            BuildPageModel model = new BuildPageModel();
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            model.Build = ViewBuild.Copy(dataLayer.GetBuildById(buildid));
            if (model.Build == null)
                return Responses.NotFoundError($"build {buildid} does not exist");

            model.Build.Job = ViewJob.Copy(dataLayer.GetJobById(model.Build.JobId));

            BuildServer buildServer = dataLayer.GetBuildServerById(model.Build.Job.BuildServerId);
            IBuildServerPlugin buildServerPlugin = pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;

            model.BuildInvolvements = ViewBuildInvolvement.Copy(dataLayer.GetBuildInvolvementsByBuild(buildid));

            foreach (ViewBuildInvolvement bi in model.BuildInvolvements)
            {
                bi.Build = model.Build;
                if (bi.RevisionId != null)
                    bi.Revision = dataLayer.GetRevisionById(bi.RevisionId);

                if (bi.MappedUserId != null)
                    bi.MappedUser = ViewUser.Copy(dataLayer.GetUserById(bi.MappedUserId));
            }

            // sorty invovlvements by revision date so they look ordered
            if (!model.BuildInvolvements.Where(bi => bi.Revision == null).Any())
                model.BuildInvolvements = model.BuildInvolvements.OrderByDescending(bi => bi.Revision.Created);

            model.UrlOnBuildServer = buildServerPlugin.GetBuildUrl(buildServer, model.Build);
            model.BuildServer = buildServer;
            model.PreviousBuild = dataLayer.GetPreviousBuild(model.Build);
            model.NextBuild = dataLayer.GetNextBuild(model.Build);
            model.BuildParseResults = dataLayer.GetBuildLogParseResultsByBuildId(buildid);
            model.Build.IncidentBuild = string.IsNullOrEmpty(model.Build.IncidentBuildId) ? null : ViewBuild.Copy(dataLayer.GetBuildById(model.Build.IncidentBuildId));
            model.RevisionsLinkedFromLog = !string.IsNullOrEmpty(model.Build.Job.RevisionAtBuildRegex);
            IEnumerable<DaemonTask> buildTasks = dataLayer.GetDaemonsTaskByBuild(model.Build.Id);
            model.ProcessErrors = buildTasks.Any(t => t.HasPassed.HasValue && t.HasPassed.Value == false);
            model.ProcessesPending = buildTasks.Any(t => t.ProcessedUtc == null);

            return View(model);
        }

        [ServiceFilter(typeof(ViewStatus))]
        [Route("/BuildProcessLog/{buildid}")]
        public IActionResult BuildProcessLog(string buildid)
        {
            BuildProcessPageModel model = new BuildProcessPageModel();
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            model.Build = ViewBuild.Copy(dataLayer.GetBuildById(buildid));
            if (model.Build == null)
                return Responses.NotFoundError($"build {buildid} does not exist");

            model.DaemonTasks = dataLayer.GetDaemonsTaskByBuild(model.Build.Id).OrderBy(t => t.Order);

            return View(model);
        }

        [ServiceFilter(typeof(ViewStatus))]
        [Route("/build/log/{buildid}")]
        public IActionResult BuildLog(string buildid)
        {
            BuildLogParseResultsPageModel model = new BuildLogParseResultsPageModel();
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
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

        [ServiceFilter(typeof(ViewStatus))]
        [Route("/processlog/{page?}")]
        public IActionResult ProcessLog(string hostname, int page, string filterby)
        {
            Configuration config = _di.Resolve<Configuration>();
            hostname = HttpUtility.UrlDecode(hostname);
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            DaemonActiveProcesses activeItems = _di.Resolve<DaemonActiveProcesses>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            ProcessPageModel model = new ProcessPageModel();

            model.ActiveProcesses = activeItems.GetCurrent();
            model.DaemonTasks = ViewDaemonTask.Copy(dataLayer.PageDaemonTasks(page > 0 ? page - 1 : page, config.StandardPageSize, filterby));
            model.DaemonTasks.Items.ToList().ForEach(daemonTask => daemonTask.Build = ViewBuild.Copy(dataLayer.GetBuildById(daemonTask.BuildId)));
            model.BaseUrl = $"/processlog";
            model.QueryStrings = $"filterby={filterby}";

            return View(model);
        }

        [ServiceFilter(typeof(ViewStatus))]
        [Route("/buildhost/{hostname}/{page?}")]
        public IActionResult BuildHost(string hostname, int page)
        {
            Configuration config = _di.Resolve<Configuration>();
            hostname = HttpUtility.UrlDecode(hostname);
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            BuildHostModel model = new BuildHostModel();

            model.Hostname = hostname;
            model.Builds = ViewBuild.Copy(dataLayer.PageBuildsByBuildAgent(hostname, page > 0 ? page - 1 : page, config.StandardPageSize));
            model.Builds.Items.ToList().ForEach(build => build.Job = ViewJob.Copy(dataLayer.GetJobById(build.JobId)));
            model.BaseUrl = $"/buildhost/{hostname}"; 

            return View(model);
        }

        [ServiceFilter(typeof(ViewStatus))]
        [Route("/user/{userid}")]
        public new IActionResult User(string userid)
        {
            UserPageModel model = new UserPageModel();
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
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
        [ServiceFilter(typeof(ViewStatus))]
        [Route("/users")]
        public IActionResult Users()
        {
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            UsersPageModel model = new UsersPageModel();
            model.Users = dataLayer.GetUsers();
            return View(model);
        }

        /// <summary>
        /// Lists remote jobs on build server, NOT jobs in local db
        /// </summary>
        /// <param name="buildserverid"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(ViewStatus))]
        [Route("/jobs/{buildserverid}")]
        public IActionResult Jobs(string buildserverid)
        {
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            BuildServer buildServer = dataLayer.GetBuildServerByKey(buildserverid);

            if (buildServer == null)
                return Redirect("buildserver not found");

            IBuildServerPlugin buildServerPlugin = pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
            IEnumerable<string> jobs = buildServerPlugin.ListRemoteJobsCanonical(buildServer);

            ViewData["jobs"] = jobs;
            ViewData["buildServer"] = buildServer;

            LayoutModel model = new LayoutModel();
            return View(model);
        }

        [ServiceFilter(typeof(ViewStatus))]
        [Route("/build/reprocesslog/{buildid}")]
        public string ReprocessLog(string buildid)
        {
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Build build = dataLayer.GetBuildById(buildid);
            Job job = dataLayer.GetJobById(build.JobId);
            List<ILogParserPlugin> logParsers = new List<ILogParserPlugin>();
            SimpleDI di = new SimpleDI();
            BuildLogParseResultHelper buildLogParseResultHelper = di.Resolve<BuildLogParseResultHelper>();
            foreach (string logParserName in job.LogParserPlugins)
                logParsers.Add(pluginProvider.GetByKey(logParserName) as ILogParserPlugin);

            // delete existing log processes
            foreach(BuildLogParseResult logParseResult in dataLayer.GetBuildLogParseResultsByBuildId(buildid))
                dataLayer.DeleteBuildLogParseResult(logParseResult);

            foreach (ILogParserPlugin logParser in logParsers)
                buildLogParseResultHelper.ProcessBuild(dataLayer, build, logParser, _log);

            return string.Empty;
        }


        #endregion
    }
}
