using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public delegate void DaemonWorkThreaded(IDataPlugin dataRead, IDataPlugin dataWrite, DaemonTask task, Build build, Job job);
}
