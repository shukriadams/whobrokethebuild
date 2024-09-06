namespace Wbtb.Core.Web
{
    public interface IDaemonTaskController
    {
        void Start(DaemonWorkThreaded work, int tickInterval, IWebDaemon daemon, DaemonTaskTypes? daemonLevel);
        
        void Start(DaemonWork work, int tickInterval);

        void Dispose();
    }
}
