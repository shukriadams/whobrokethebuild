using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class ViewIncidentCauseBuild : Build
    {
        /// <summary>
        /// Time between first and last build in incident
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Builds incident that are not the cause build
        /// </summary>
        public IEnumerable<Build> InvolvedBuilds { get; set; }

        public static ViewIncidentCauseBuild Copy(Build build)
        {
            if (build == null)
                return null;

            SimpleDI di = new SimpleDI();
            PluginProvider plugins = di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = plugins.GetFirstForInterface<IDataPlugin>();

            IEnumerable<Build> involved = dataLayer.GetBuildsByIncident(build.Id);

            TimeSpan duration = TimeSpan.Zero;

            return new ViewIncidentCauseBuild
            {
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
                Id = build.Id,
                Duration = duration,
                InvolvedBuilds = involved
            };
        }

        public static PageableData<ViewIncidentCauseBuild> Copy(PageableData<Build> builds)
        {
            return new PageableData<ViewIncidentCauseBuild>(
                builds.Items.Select(r => ViewIncidentCauseBuild.Copy(r)),
                builds.PageIndex,
                builds.PageSize,
                builds.TotalItemCount);
        }
    }
}
