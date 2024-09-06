namespace Wbtb.Core.Web
{
    public interface IDaemonTaskController
    {
        void WatchForAndRunTasksForDaemon(IWebDaemon daemon, int tickInterval, DaemonTaskTypes? daemonLevel);
        
        void WatchForAndRunTasksForDaemon(IWebDaemon work, int tickInterval);

        void Dispose();
    }
}
