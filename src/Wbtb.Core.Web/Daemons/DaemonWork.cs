namespace Wbtb.Core.Web
{
    /// <summary>
    /// Defines a generic method on a daemon that does whatever work that daemon is meant to do. This method is called by the timer controller for a daemon, 
    /// letting us separate regular and consistent timers from the work done the timer. It also lets us test daemon work logic without having to deal with 
    /// timers. Timers are tested indepedently.
    /// </summary>
    public delegate void DaemonWork();
}
