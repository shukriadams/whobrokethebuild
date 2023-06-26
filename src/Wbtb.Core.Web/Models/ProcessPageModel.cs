using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class ProcessPageModel : LayoutModel
    {
        #region PROPERTIES

        public PageableData<ViewDaemonTask> DaemonTasks { get; set; }

        public string BaseUrl { get; set; }

        #endregion

        #region CTORS

        public ProcessPageModel() 
        {
            this.DaemonTasks = new PageableData<ViewDaemonTask>(new ViewDaemonTask[] { }, 0, 0, 0);
        }

        #endregion
    }
}
