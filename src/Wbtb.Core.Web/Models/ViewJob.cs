using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class ViewJob : Job
    {
        /// <summary>
        /// Carrier property, must be set at runtime. 
        /// </summary>
        public Build LatestBuild { get; set; }
        
        public ViewBuild BreakBuild { get; set; }

        /// <summary>
        /// Last build in job to set delta. can be null.
        /// </summary>
        public Build DeltaBuild { get; set; }

        public static ViewJob Copy(Job job)
        {
            if (job == null)
                return null;

            return new ViewJob{ 
                Message = job.Message,
                BuildServer = job.BuildServer,
                Description = job.Description,
                Enable = job.Enable,
                HistoryLimit = job.HistoryLimit,
                Key = job.Key,
                Image = job.Image,
                ImportCount = job.ImportCount,
                LogParserPlugins = job.LogParserPlugins,
                Name = job.Name,
                SourceServer = job.SourceServer,
                BuildServerId = job.BuildServerId,
                Id = job.Id,
                SourceServerId = job.SourceServerId
            };
        }
    }
}
