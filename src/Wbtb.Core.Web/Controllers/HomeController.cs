﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
                ViewJob viewJob = ViewJob.Copy(job);
                tempjobs.Add(viewJob);
                viewJob.LatestBuild = dataLayer.GetLatestBuildByJob(job);
                
                // TODO : this doesn't make sense anymore, getting by deltabuild, or getting involvements 
                viewJob.DeltaBuild = ViewBuild.Copy(dataLayer.GetLastJobDelta(job.Id));
                if (viewJob.DeltaBuild != null)
                    viewJob.DeltaBuild.BuildInvolvements = ViewBuildInvolvement.Copy(dataLayer.GetBuildInvolvementsByBuild(viewJob.DeltaBuild.Id));
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
        [Route("/incident/{publicBuildId}")]
        public IActionResult Incident(string publicBuildId)
        {
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();

            IncidentPageModel model = new IncidentPageModel();
            model.IncidentBuild = dataLayer.GetBuildByUniquePublicIdentifier(publicBuildId);
            if (model.IncidentBuild == null)
                return Responses.NotFoundError($"build {publicBuildId} does not exist");

            if (model.IncidentBuild != null)
                model.FixingBuild = dataLayer.GetFirstPassingBuildAfterBuild(model.IncidentBuild);

            Build jobDelta = dataLayer.GetLastJobDelta(model.IncidentBuild.JobId);
            if (jobDelta != null && jobDelta.IncidentBuildId == model.IncidentBuild.Id)
                model.IsActive = true;

            model.Job = dataLayer.GetJobById(model.IncidentBuild.JobId);
            model.InvolvedBuilds = dataLayer.GetBuildsByIncident(model.IncidentBuild.Id);
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
            ConsoleHelper.WriteLine($"Reset {deleted} incident from build {buildId}");

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
            model.Banner = new ViewJobBanner { Job = model.Job };
            model.Banner.BreadCrumbs.Add(ViewHelpers.String(model.Job.Name));

            foreach (ViewBuild build in model.Builds.Items)
            {
                build.MutationReport = dataLayer.GetMutationReportByBuild(build.Id);
                build.BuildInvolvements = ViewBuildInvolvement.Copy(dataLayer.GetBuildInvolvementsByBuild(build.Id)).OrderByDescending(bi => bi.RevisionCode);
                if (build.EndedUtc.HasValue)
                    build.Duration = build.EndedUtc.Value - build.StartedUtc;
            }

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
        [Route("/build/{publicBuildId}")]
        public IActionResult Build(string publicBuildId)
        {
            BuildPageModel model = new BuildPageModel();
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            model.Build = ViewBuild.Copy(dataLayer.GetBuildByUniquePublicIdentifier(publicBuildId));
            if (model.Build == null)
                return Responses.NotFoundError($"build {publicBuildId} does not exist");

            model.Build.Job = ViewJob.Copy(dataLayer.GetJobById(model.Build.JobId));
            model.Banner = new ViewJobBanner { Job = model.Build.Job };
            model.Banner.BreadCrumbs.Add(ViewHelpers.JobLink(model.Build.Job));
            model.Banner.BreadCrumbs.Add(ViewHelpers.String(model.Build.Key));

            IEnumerable<DaemonTask> daemonTasks = dataLayer.GetDaemonTasksByBuild(model.Build.Id);

            BuildServer buildServer = dataLayer.GetBuildServerById(model.Build.Job.BuildServerId);
            IBuildServerPlugin buildServerPlugin = pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;

            model.BuildInvolvements = ViewBuildInvolvement.Copy(dataLayer.GetBuildInvolvementsByBuild(model.Build.Id));
            
            /// if build should have a revision from build but doesn't have it yet, try to explain why
            if (model.Build.Job.CanHaveRevisionInBuildLog && string.IsNullOrEmpty(model.Build.RevisionInBuildLog)) 
            {
                DaemonTask task = daemonTasks.FirstOrDefault(t => t.Stage == (int)DaemonTaskTypes.RevisionFromLog);
                if (task == null)
                {
                    model.Build.RevisionInBuildLog = "No daemon task to read";
                }
                else 
                {
                    if (task.ProcessedUtc == null)
                        model.Build.RevisionInBuildLog = "Not processed yet";
                    else if (task.HasPassed.HasValue && task.HasPassed.Value== false)
                        model.Build.RevisionInBuildLog = "Log parse failed";
                    else
                        model.Build.RevisionInBuildLog = "Error reading from log";
                }
            }

            foreach (ViewBuildInvolvement bi in model.BuildInvolvements)
            {
                bi.Build = model.Build;
                if (bi.RevisionId != null)
                    bi.Revision = dataLayer.GetRevisionById(bi.RevisionId);

                if (bi.MappedUserId != null)
                    bi.MappedUser = ViewUser.Copy(dataLayer.GetUserById(bi.MappedUserId));
            }

            // sort invovlvements by revision date so they look ordered
            if (!model.BuildInvolvements.Where(bi => bi.Revision == null).Any())
                model.BuildInvolvements = model.BuildInvolvements.OrderByDescending(bi => bi.Revision.Created);

            model.PreviousIncident = dataLayer.GetPreviousIncident(model.Build);
            model.UrlOnBuildServer = buildServerPlugin.GetBuildUrl(buildServer, model.Build);
            model.BuildServer = buildServer;
            model.PreviousBuild = dataLayer.GetPreviousBuild(model.Build);
            model.NextBuild = dataLayer.GetNextBuild(model.Build);
            model.BuildParseResults = ViewBuildLogParseResult.Copy(dataLayer.GetBuildLogParseResultsByBuildId(model.Build.Id).OrderBy(r => string.IsNullOrEmpty(r.ParsedContent)));

            model.Build.IncidentBuild = string.IsNullOrEmpty(model.Build.IncidentBuildId) ? null : ViewBuild.Copy(dataLayer.GetBuildById(model.Build.IncidentBuildId));
            model.RevisionsLinkedFromLog = !string.IsNullOrEmpty(model.Build.Job.RevisionAtBuildRegex);
            model.ProcessErrors = daemonTasks.Any(t => t.HasPassed.HasValue && t.HasPassed.Value == false);
            model.ProcessesPending = daemonTasks.Any(t => t.ProcessedUtc == null);
            model.MutationReport = dataLayer.GetMutationReportByBuild(model.Build.Id);

            return View(model);
        }


        [ServiceFilter(typeof(ViewStatus))]
        [Route("/BuildProcessLog/{publicBuildId}")]
        public IActionResult BuildProcessLog(string publicBuildId)
        {
            BuildProcessPageModel model = new BuildProcessPageModel();
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            DaemonTaskProcesses daemonTaskProcesses = _di.Resolve<DaemonTaskProcesses>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            model.Build = ViewBuild.Copy(dataLayer.GetBuildByUniquePublicIdentifier(publicBuildId));
            if (model.Build == null)
                return Responses.NotFoundError($"build {publicBuildId} does not exist");

            model.DaemonTasks = ViewDaemonTask.Copy(dataLayer.GetDaemonTasksByBuild(model.Build.Id).OrderBy(t => t.CreatedUtc));
            model.IsComplete = !model.DaemonTasks.Any(t => t.ProcessedUtc == null);
            model.HasErrors = model.DaemonTasks.Any(t => t.HasPassed == false);
            model.Banner = new ViewJobBanner { Job = ViewJob.Copy(dataLayer.GetJobById(model.Build.JobId)) };
            model.Banner.BreadCrumbs.Add(ViewHelpers.JobLink(model.Banner.Job));
            model.Banner.BreadCrumbs.Add(ViewHelpers.BuildLink(model.Build));
            model.Banner.BreadCrumbs.Add(ViewHelpers.String("Process Log"));

            if (model.DaemonTasks.Any()) 
            {
                if (model.IsComplete)
                    model.QueueTime = model.DaemonTasks.OrderByDescending(t => t.CreatedUtc).First().ProcessedUtc.Value - model.DaemonTasks.OrderBy(t => t.CreatedUtc).First().CreatedUtc;
                else
                    model.QueueTime = DateTime.UtcNow - model.DaemonTasks.OrderBy(t => t.CreatedUtc).First().CreatedUtc;
            }

            foreach (ViewDaemonTask task in model.DaemonTasks) 
            {
                task.ActiveProcess = daemonTaskProcesses.GetActive(task);
                task.BlockedProcess = daemonTaskProcesses.GetBlocked(task);
            }

            return View(model);
        }


        [ServiceFilter(typeof(ViewStatus))]
        [Route("/build/log/{publicBuildId}")]
        public IActionResult BuildLog(string publicBuildId)
        {
            BuildLogParseResultsPageModel model = new BuildLogParseResultsPageModel();
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            Configuration config = _di.Resolve<Configuration>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            model.Build = ViewBuild.Copy(dataLayer.GetBuildByUniquePublicIdentifier(publicBuildId));
            if (model.Build == null)
                return Responses.NotFoundError($"build {publicBuildId} does not exist");

            Job job = dataLayer.GetJobById(model.Build.JobId);
            model.Banner = new ViewJobBanner { Job = ViewJob.Copy(dataLayer.GetJobById(model.Build.JobId)) };
            model.Banner.BreadCrumbs.Add(ViewHelpers.JobLink(model.Banner.Job));
            model.Banner.BreadCrumbs.Add(ViewHelpers.BuildLink(model.Build));
            model.Banner.BreadCrumbs.Add(ViewHelpers.String("Log"));

            if (model.Build.LogFetched)
            {
                string logPath = Common.Build.GetLogPath(config, job, model.Build);
                if (System.IO.File.Exists(logPath))
                    model.Raw = System.IO.File.ReadAllText(logPath);
                else
                    return Responses.UnknownError($"The log for build {publicBuildId} was not found at expected path {logPath}", 120); // todo : proper code needed
            }

            return View(model);
        }

        [ServiceFilter(typeof(ViewStatus))]
        [Route("/processlog/{page?}")]
        public IActionResult ProcessLog(string hostname, int page, string orderBy, string filterby, string jobid)
        {
            Configuration config = _di.Resolve<Configuration>();
            hostname = HttpUtility.UrlDecode(hostname);
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            DaemonTaskProcesses daemonProcesses = _di.Resolve<DaemonTaskProcesses>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            ProcessPageModel model = new ProcessPageModel();

            // try to assign blocks to acitve processses to make it easier 
            model.DaemonTasks = ViewDaemonTask.Copy(dataLayer.PageDaemonTasks(page > 0 ? page - 1 : page, config.StandardPageSize, orderBy, filterby, jobid));
            foreach (ViewDaemonTask task in model.DaemonTasks.Items) 
            {
                DaemonBlockedProcess block = model.BlockedProcesses.FirstOrDefault(b => b.Task.Id == task.Id);
                if (block == null)
                    continue;

                model.BlockedProcesses.Remove(block);
                task.BlockedProcess = block;
            }

            

            model.DaemonTasks.Items.ToList().ForEach(daemonTask => daemonTask.Build = ViewBuild.Copy(dataLayer.GetBuildById(daemonTask.BuildId)));
            model.BaseUrl = $"/processlog";
            model.QueryStrings = $"filterby={filterby}&orderBy={orderBy}&jobid={jobid}";
            model.FilterBy = filterby;
            model.OrderBy = orderBy;
            model.JobId = jobid;
            model.Jobs = dataLayer.GetJobs().OrderBy(j => j.Name );

            return View(model);
        }

        [ServiceFilter(typeof(ViewStatus))]
        [Route("/processlogblock")]
        public IActionResult ProcessLogBlock(string hostname)
        {
            Configuration config = _di.Resolve<Configuration>();
            hostname = HttpUtility.UrlDecode(hostname);
            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            DaemonTaskProcesses daemonProcesses = _di.Resolve<DaemonTaskProcesses>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            ProcessPageModel model = new ProcessPageModel();

            DateTime timecutoff = DateTime.UtcNow - new TimeSpan(0, 0 , 1);
            model.ActiveProcesses = daemonProcesses.GetAllActive().Where(p => p.CreatedUtc < timecutoff);
            model.BlockedProcesses = daemonProcesses.GetAllBlocked().OrderByDescending(p => p.CreatedUtc).ToList();
            model.DoneProcesses = daemonProcesses.GetDone();
            IList<DaemonTask> blockedTasks = dataLayer.GetBlockingDaemonTasks().ToList();

            model.BlockingDaemonTasks = blockedTasks;
            model.DaemonTasks.Items.ToList().ForEach(daemonTask => daemonTask.Build = ViewBuild.Copy(dataLayer.GetBuildById(daemonTask.BuildId)));

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
                return Responses.NotFoundError("buildserver not found");

            IBuildServerPlugin buildServerPlugin = pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
            IEnumerable<string> jobs = buildServerPlugin.ListRemoteJobsCanonical(buildServer);

            ViewData["jobs"] = jobs;
            ViewData["buildServer"] = buildServer;

            LayoutModel model = new LayoutModel();
            return View(model);
        }

        #endregion
    }
}
