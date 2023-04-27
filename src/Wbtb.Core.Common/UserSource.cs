namespace Wbtb.Core.Common
{
    /// <summary>
    /// Used to associate a WBTB user with a user in a source control server's commits
    /// </summary>
    public class UserSourceIdentity
    {
        /// <summary>
        /// source server id
        /// </summary>
        public string SourceServerKey { get; set; }

        /// <summary>
        /// id of user as they appear in commit messages
        /// </summary>
        public string Name { get; set; }
    }
}
