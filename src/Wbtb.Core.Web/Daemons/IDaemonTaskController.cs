namespace Wbtb.Core.Web
{
    public interface IDaemonTaskController
    {
        void WatchForAndRunTasksForDaemon(DaemonWorkThreaded work, int tickInterval, IWebDaemon daemon, DaemonTaskTypes? daemonLevel);
        
        void WatchForAndRunTasksForDaemon(DaemonWork work, int tickInterval);

        void Dispose();
    }
}
