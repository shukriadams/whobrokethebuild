using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildPageModel : LayoutModel
    {
        public bool RevisionsLinkedFromLog { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// Build this model focuses on.
        /// </summary>
        public ViewBuild Build { get; set; }

        public BuildServer BuildServer { get; set; }

        /// <summary>
        /// Url on build server
        /// </summary>
        public string UrlOnBuildServer { get; set; }

        /// <summary>
        /// If focus build is part of a breaking incident, this is the build responsible for incident. Null if not known / no incident
        /// </summary>
        public ViewBuild IncidentCausalBuild { get; set; }

        /// <summary>
        /// Confirmed build breakers
        /// </summary>
        public IEnumerable<User> BuildBreakers { get; set; }

        public IEnumerable<BuildProcessor> buildProcessors { get; set; }

        public CommonModel Common { get;set; }

        public Build PreviousBuild { get; set; }

        public Build NextBuild { get; set; }

        public IEnumerable<BuildFlag> BuildFlags { get; set; }

        /// <summary>
        /// true if broken alerts for this build have been sent, and cane be retracted
        /// </summary>
        public bool IsAlertRetractable {get;set; }

        /// <summary>
        /// All people involved in build break
        /// </summary>
        public IEnumerable<ViewBuildInvolvement> BuildInvolvements { get;set;}

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<BuildLogParseResult> BuildParseResults { get; set; }

        public bool ProcessErrors { get; set; }

        public bool ProcessesPending { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BuildPageModel()
        { 
            this.BuildBreakers = new List<User>();
            this.Common = new CommonModel();
            this.BuildInvolvements = new List<ViewBuildInvolvement>();
            this.BuildFlags = new List<BuildFlag>();
            this.buildProcessors = new List<BuildProcessor>();
        }
    }
}
