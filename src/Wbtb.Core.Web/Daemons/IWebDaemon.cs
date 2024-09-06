using Wbtb.Core.Common;

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
        /// <param name="interval">Time in milliseconds. Set in config. Overridden by this.Interval if this.Interval returns non-zero. </param>
        void Start(int interval);
    }
}
