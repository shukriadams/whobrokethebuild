using System;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Compount data object for a build break.
    /// </summary>
    public class Incident
    {
        public Build CauseBuild { get; set; }

        public Build LastBuild { get; set; }

        public Build ResolvingBuild { get; set; }

        public bool Resolved { get; set; }

        public int BuildsInIncident { get; set; }

        public TimeSpan? Duration { get; set; }
    }
}
