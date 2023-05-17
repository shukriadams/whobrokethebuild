﻿using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Extends builds, adds data used to display build on HTML page
    /// </summary>
    public class ViewBuild : Build
    {
        public IEnumerable<ViewBuildInvolvement> BuildInvolvements { get; set; } 

        public bool IsResonsibleForBreakingBuild { get; set; }

        public ViewBuild PreviousBuild { get; set; }

        public ViewBuild NextBuild { get; set;}

        public ViewBuild IncidentBuild { get; set; }

        public ViewJob Job { get; set; }

        public ViewBuild()
        {
            BuildInvolvements = new List<ViewBuildInvolvement>();
        }

        /// <summary>
        /// Simple single-layer conversion of database-level build to display-level build
        /// </summary>
        /// <param name="build"></param>
        /// <returns></returns>
        public static ViewBuild Copy(Build build)
        { 
            if (build == null)
                return null;

            return new ViewBuild{ 
                EndedUtc = build.EndedUtc,
                Hostname = build.Hostname,
                Identifier = build.Identifier,
                IncidentBuildId = build.IncidentBuildId,
                LogPath = build.LogPath,
                JobId = build.JobId,
                StartedUtc = build.StartedUtc,
                Status = build.Status,
                TriggeringCodeChange = build.TriggeringCodeChange,
                TriggeringType = build.TriggeringType,
                Id = build.Id
            };
        }

        public static PageableData<ViewBuild> Copy(PageableData<Build> builds)
        {
            return new PageableData<ViewBuild>(
                builds.Items.Select(r => ViewBuild.Copy(r)),
                builds.PageIndex,
                builds.PageSize,
                builds.TotalItemCount);
        }
    }
}