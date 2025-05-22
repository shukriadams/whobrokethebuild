using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public interface IDaemonTaskController
    {
        void WatchForAndRunTasksForDaemon(IWebDaemon daemon, int tickInterval, ProcessStages? daemonLevel);
        
        void WatchForAndRunTasksForDaemon(IWebDaemon work, int tickInterval);

        void Dispose();
    }
}
