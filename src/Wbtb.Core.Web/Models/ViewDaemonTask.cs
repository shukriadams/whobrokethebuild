using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class ViewDaemonTask : DaemonTask
    {
        public Build Build { get; set; }

        public DaemonBlockedProcessItem Block { get; set; }

        public static ViewDaemonTask Copy(DaemonTask daemonTask)
        {
            if (daemonTask == null)
                return null;

            return new ViewDaemonTask
            {
                BuildId = daemonTask.BuildId,
                BuildInvolvementId = daemonTask.BuildInvolvementId,
                CreatedUtc = daemonTask.CreatedUtc,
                HasPassed = daemonTask.HasPassed,
                Id = daemonTask.Id,
                Order = daemonTask.Order,
                ProcessedUtc = daemonTask.ProcessedUtc,
                Result = daemonTask.Result,
                Signature = daemonTask.Signature,
                Src = daemonTask.Src,
                TaskKey = daemonTask.TaskKey
            };
        }

        public static PageableData<ViewDaemonTask> Copy(PageableData<DaemonTask> items)
        {
            return new PageableData<ViewDaemonTask>(
                items.Items.Select(r => ViewDaemonTask.Copy(r)),
                items.PageIndex,
                items.PageSize,
                items.TotalItemCount);
        }
    }
}
