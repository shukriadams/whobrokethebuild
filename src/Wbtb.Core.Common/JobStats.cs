using System;

namespace Wbtb.Core.Common
{
    public class JobStats
    {
        /// <summary>
        /// 
        /// </summary>
        public Build LatestBuild { get;set;}

        /// <summary>
        /// Time between first and latest build
        /// </summary>
        public TimeSpan? JobDuration { get; set; }
 
        /// <summary>
        /// Combined time of all failing durations
        /// </summary>
        public TimeSpan? TotalDowntime { get; set; }

        public DateTime? StartUtc { get; set; }

        /// <summary>
        /// Build at the start of latest breaking streak
        /// </summary>
        public Build LatestBreakingBuild { get; set; }

        /// <summary>
        /// The absolute last breaking build
        /// </summary>
        public Build LatestBrokenBuild { get; set; }

        /// <summary>
        /// Total Time job was down last
        /// </summary>
        public TimeSpan? LatestBreakDuration { get; set; }

        /// <summary>
        /// Total number of times job has built
        /// </summary>
        public int TotalBuilds { get; set; }

        public int Incidents { get; set; }

        /// <summary>
        /// Total number of times builds have not passed (includes aborts and others)
        /// </summary>
        public int TotalFails { get; set; }

        /// <summary>
        /// Percentage of builds that fail
        /// </summary>
        public float FailRatePercent { get; set; }

        /// <summary>
        /// Average number of builds per day
        /// </summary>
        public int DailyBuildRate { get; set; }

        /// <summary>
        /// Average number of builds per week
        /// </summary>
        public int WeeklyBuildRate { get; set; }

        /// <summary>
        /// Longest unbroken updatime
        /// </summary>
        public TimeSpan? LongestUpdatetime { get; set; }

        /// <summary>
        /// Longest duration
        /// </summary>
        public TimeSpan? LongestDowntime { get; set; }
    }
}
