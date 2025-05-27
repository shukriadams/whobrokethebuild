using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class ViewDaemonTask : DaemonTask
    {
        #region PROPERTIES

        public Build Build { get; set; }

        public DaemonActiveProcess ActiveProcess { get; set; }

        public DaemonBlockedProcess BlockedProcess { get; set; }

        #endregion

        #region METHODS

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
                Stage = daemonTask.Stage,
                ProcessedUtc = daemonTask.ProcessedUtc,
                Result = daemonTask.Result,
                Signature = daemonTask.Signature,
                Src = daemonTask.Src
            };
        }

        public static IEnumerable<ViewDaemonTask> Copy(IEnumerable<DaemonTask> items)
        {
            return items.Select(r => Copy(r));
        }

        public static PageableData<ViewDaemonTask> Copy(PageableData<DaemonTask> items)
        {
            return new PageableData<ViewDaemonTask>(
                items.Items.Select(r => ViewDaemonTask.Copy(r)),
                items.PageIndex,
                items.PageSize,
                items.TotalItemCount);
        }

        #endregion
    }
}
