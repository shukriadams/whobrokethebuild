namespace Wbtb.Core.Common
{
    public class AuthenticationResult
    {
        /// <summary>
        /// If auth approved, User object that logged in
        /// </summary>
        public User User { get;set; }

        /// <summary>
        /// True if login succeeded
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Text from auth provider describing outcome of login.
        /// </summary>
        public string Message { get; set; }
    }
}
