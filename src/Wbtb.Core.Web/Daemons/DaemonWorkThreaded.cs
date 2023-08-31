using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public delegate DaemonTaskWorkResult DaemonWorkThreaded(IDataPlugin dataRead, IDataPlugin dataWrite, DaemonTask task, Build build, Job job);
}
