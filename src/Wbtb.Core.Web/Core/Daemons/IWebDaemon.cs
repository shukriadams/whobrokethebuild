namespace Wbtb.Core.Web
{
    /// <summary>
    /// Defines a type run by core web server hat performs daemon process
    /// </summary>
    public interface IWebDaemon
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval">Time in milliseconds</param>
        public void Start(int interval);
    }
}
