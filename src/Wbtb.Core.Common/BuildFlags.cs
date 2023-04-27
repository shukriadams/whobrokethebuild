namespace Wbtb.Core.Common
{
    public enum BuildFlags : int
    {
        None = 0,

        /// <summary>
        /// Build is meant to report revision at time of build in its log, but no revision was found. 
        /// </summary>
        LogHasNoRevision = 1,

        /// <summary>
        /// Revision code exists in build, but could not be found on source control server associated with build job.
        /// </summary>
        RevisionNotFound = 2,

        /// <summary>
        /// User defined in string in buildinvolvement is not tied to a WBTB user. User should be created locally, and associated
        /// with the string name in the buildinvolvement
        /// </summary>
        BuildUserNotDefinedLocally = 3,

        /// <summary>
        /// Build failed, but could not be coupled together with earlier/later builds to assign an incident number.
        /// </summary>
        IncidentBuildLinkError = 4,

        /// <summary>
        /// Build Log parsing has failed
        /// </summary>
        LogParseFailed = 5
    }
}
