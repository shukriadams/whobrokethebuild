namespace Wbtb.Core.Web
{
    public interface IDaemonProcessRunner
    {
        void Start(DaemonWorkThreaded work, int tickInterval, IWebDaemon daemon, DaemonTaskTypes? daemonLevel);
        
        void Start(DaemonWork work, int tickInterval);

        void Dispose();
    }
}
